using DatagramsNet.Datagram;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.ElementHelper;
using TeleBufet.NET.ElementHelper.Elements;
using TeleBufet.NET.Pages.ShoppingCartPage;
using TeleBufet.NET.Pages.TicketPage;

namespace TeleBufet.NET.Pages.ProductPage;

public enum Operator : int
{
	Minus = -1,
	Plus = 1
}

public partial class MainProductPage : ContentPage
{
	private FlexLayout productLayout;
	private HorizontalStackLayout categoryLayout;

	private ReadOnlyMemory<Frame> productFrames;
	private Memory<ProductElement> productsElements;
	private ReadOnlyMemory<ProductInformationHolder> products;

	public static UserTable User { get; set; }

	public RefreshView MainRefreshView { get; } = new();

	public MainProductPage()
	{
		InitializeComponent();
		ShoppingCart.Clicked += async(object sender, EventArgs e) => await Navigation.PushModalAsync(new CartPage());
		Tickets.Clicked += async(object sender, EventArgs e) => await Navigation.PushModalAsync(new ReservationPage());

		productLayout = collection;
		categoryLayout = categoryStackLayout;

		var oldTimeSpan = ExtendedClient.lastRequest;
		_ = ConditionTask.WaitUntil(new Func<bool>(() => oldTimeSpan == ExtendedClient.lastRequest), 10);

		CreateProducts();
		CreateCategories();
		SetCategory(null);

		_ = UpdateElements();
		MainRefreshView.Command = new Command(async() =>
		{
			await UpdateElements();
			MainRefreshView.IsRefreshing = false;
		});
	}

	private async Task UpdateElements() 
	{
		using var informationTableCacheManager = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
		var updatePacketRequest = new RequestProductInformationPacket()
		{
			ProductInformationTables = informationTableCacheManager.Deserialize()
		};
		var oldTimeSpan = ExtendedClient.lastRequest;
		await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(updatePacketRequest));

		_ = ConditionTask.WaitUntil(new Func<bool>(() => oldTimeSpan == ExtendedClient.lastRequest), 10);

		using var newInformationTableCacheManager = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
		var newData = newInformationTableCacheManager.Deserialize();

        for (int i = 0; i < newData.Length; i++)
        {
			int index = newData[i].Id - 1;
			var currentElement = productsElements.Span[index];
			if (ProductElement.TryUpdateElement(ref currentElement, newData[i])) 
			{
				productsElements.Span[index] = currentElement;
				Device.BeginInvokeOnMainThread(async () => await App.Current.MainPage.DisplayAlert("Update", $"Product {products.Span[index].Product.Name} was updated", "Ok"));
			}
        }
	}

	private bool ProductEqual(ProductInformationTable[] firstTable, ProductInformationTable[] secondTable) 
	{
		if (firstTable.Length != secondTable.Length)
			return false;
        for (int i = 0; i < firstTable.Length; i++)
        {
			double firstIndex = firstTable[i].Amount + firstTable[i].Price;
			double secondIndex = secondTable[i].Amount + secondTable[i].Price;
			if (firstIndex != secondIndex)
				return false;
        }
		return true;
	}

	private void CreateProducts() 
	{
		using var productTableCacheManager = new TableCacheHelper<ProductTable, ProductCache>();
		using var informationTableCacheManager = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();

		var elements = ProductElement.CreateElementObjects(productTableCacheManager.Deserialize(), informationTableCacheManager.Deserialize());
		products = elements;

		productsElements = CreateTableElements<ProductInformationHolder, ProductElement, StackLayout>(elements);
		productFrames = CreateTableFrames<ProductInformationHolder, ProductElement, StackLayout>(productsElements);
	}

	private void CreateCategories() 
	{
		using var categoryTableCacheManager = new TableCacheHelper<CategoryTable, CategoryCache>();

		var elements = CategoryElement.CreateElementObjects(categoryTableCacheManager.Deserialize(), SetCategory);

		var inicializeElements = CreateTableElements<ActionTable<CategoryTable>, CategoryElement, Grid>(elements);
		var frames = CreateTableFrames<ActionTable<CategoryTable>, CategoryElement, Grid>(inicializeElements);

        for (int i = 0; i < inicializeElements.Length; i++)
        {
			frames.Span[i].Margin = 0;
			categoryLayout.Children.Add(frames.Span[i]);
        }
	}

	// If table is null, it will show all products
	private void SetCategory(CategoryTable? table) 
	{
		//We know that this type of clearing is not fastest, but still this is a temporary solution
		productLayout.Children.Clear();


		ReadOnlyMemory<ProductInformationHolder> memoryTables = table is null ? products : GetProductsById(products, table.Id).ToArray();
        for (int i = 0; i < memoryTables.Length; i++)
        {
			int index = memoryTables.Span[i].Product.Id - 1;
			Device.BeginInvokeOnMainThread(() => productLayout.Children.Add(productFrames.Span[index]));
        }
	}

	private static Memory<ProductInformationHolder> GetProductsById(ReadOnlyMemory<ProductInformationHolder> tables, int id) 
	{
		Memory<ProductInformationHolder> currentTables = new ProductInformationHolder[tables.Length];
		int newProductsCount = 0;
		for (int i = 0; i < tables.Length; i++)
		{
			if (tables.Span[i].Product.CategoryId == id) 
			{
				currentTables.Span[newProductsCount] = tables.Span[i];
				newProductsCount++;
			}
        }
		return currentTables[0..newProductsCount];
	}

	private Memory<TElement> CreateTableElements<T, TElement, TLayout>(ReadOnlyMemory<T> tables) where TLayout : Microsoft.Maui.ILayout, new() where TElement : ImmutableElement<T, TLayout>, new()
	{
		Memory<TElement> elements = new TElement[tables.Length];
        for (int i = 0; i < tables.Length; i++)
        {
			var customElement = new TElement();

			customElement.Inicialize(tables.Span[i]);
			elements.Span[i] = customElement;
        }
		return elements;
	}

	private ReadOnlyMemory<Frame> CreateTableFrames<T, TElement, TLayout>(Memory<TElement> elements) where TLayout : Microsoft.Maui.ILayout, new() where TElement : ImmutableElement<T, TLayout>, new()
	{
		Memory<Frame> frames = new Frame[elements.Length];

        for (int i = 0; i < frames.Length; i++)
        {
			var currentView = (elements.Span[i].LayoutHandler as View);
			var baseFrame = new Frame()
			{
				Margin = 4,
				BackgroundColor = Color.FromHex("#f9f9fd"),
				CornerRadius = 15,
				Content = currentView
			};
			frames.Span[i] = baseFrame;
        }
		return frames;
	}
}
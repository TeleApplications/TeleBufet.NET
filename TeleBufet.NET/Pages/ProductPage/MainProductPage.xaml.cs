using DatagramsNet.Datagram;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.Interfaces;
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
	private Frame[] productFrames;

	private Memory<ProductTable> products = new();
	private Memory<CategoryTable> categories = new();

	public static UserTable User { get; set; }

	public MainProductPage()
	{
		InitializeComponent();
		ShoppingCart.Clicked += async(object sender, EventArgs e) => await Navigation.PushModalAsync(new CartPage());
		Tickets.Clicked += async(object sender, EventArgs e) => await Navigation.PushModalAsync(new ReservationPage());
		Task.Run(async() => 
		{
			while (this.IsEnabled) 
			{
				await UpdateElements();
			}
		});
		productLayout = collection;
	}

	private Command ExecuteUpdateCommand(RefreshView refreshView)
	{
		var asyncAction = (Action)(async() => await UpdateElements());
		var command = new Command(asyncAction);
		refreshView.IsRefreshing = false;

		return command;
	}

	private async Task UpdateElements() 
	{
		var cacheTable = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
		var oldProductData = cacheTable.Deserialize().ToArray();

		var requestProductInfromatios = new RequestProductInformationPacket() {ProductInformationTables = oldProductData};
		await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(requestProductInfromatios));

		var productCache = TryUpdateCacheTables<ProductTable, ProductCache>(ref products);
		if (productCache)
		{

			var newProductData = cacheTable.Deserialize().ToArray();

			var productObjects = ProductElement.CreateElementObjects(products.Span.ToArray(), newProductData).ToArray();
			var elements = GetTableElements<ProductInformationHolder, ProductElement, StackLayout>(productObjects).ToArray();
			productFrames = new Frame[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
				var baseFrame = new Frame() {CornerRadius = 15, Margin = 4, BackgroundColor = Colors.White };
				var baseHorizontalLayout = new StackLayout();

				//TODO: This needs to go into ProductElement.cs
				var addButton = new Button() { BackgroundColor = Color.FromArgb("#4cb86b"), Padding = new Thickness(0, 0, 50, 45), VerticalOptions = LayoutOptions.End, HorizontalOptions = LayoutOptions.End, WidthRequest = 45, HeightRequest = 45, CornerRadius = 10};
				var currentProduct = products.Span[i];
				addButton.Clicked += async(object sender, EventArgs e) => 
				{
					await ProductElement.ProductManipulation(currentProduct, Operator.Plus); 
				};
				addButton.IsEnabled = newProductData[i].Amount > 0;

                baseHorizontalLayout.Children.Add(elements[i]);
				baseHorizontalLayout.Children.Add(addButton);
				baseFrame.Content = baseHorizontalLayout;

				productFrames[i] = baseFrame;
            }
			//SetCategory(null);
		}
		if (TryUpdateCacheTables<CategoryTable, CategoryCache>(ref categories)) 
		{
			var categoryObject = CategoryElement.CreateElementObjects(categories.Span.ToArray(), SetCategory).ToArray();
			var categoryElements = GetTableElements<ActionTable<CategoryTable>, CategoryElement, Grid>(categoryObject).ToArray();
            for (int i = 0; i < categoryElements.Length; i++)
            {
				var categoryFrame = new Frame() {CornerRadius = 15, BackgroundColor = Colors.White, Content = categoryElements[i], WidthRequest = 60, HeightRequest = 60, VerticalOptions = LayoutOptions.StartAndExpand,  Margin = new Thickness(55,0,0,0)};
				Device.BeginInvokeOnMainThread(() => categoryStackLayout.Children.Add(categoryFrame));
            }
		}
	}


	// If table is null, it will show all products
	private void SetCategory(CategoryTable? table) 
	{
		//We know that this type of clearing is not fastest, but still this is a temporary solution
		productLayout.Children.Clear();

		Span<ProductTable> spanTables = table is null ? products.Span : GetProductsById(products, table.Id).ToArray();
        for (int i = 0; i < spanTables.Length; i++)
        {
			int index = spanTables[i].Id;
			Device.BeginInvokeOnMainThread(() => productLayout.Children.Add(productFrames[index]));
        }
	}

	private static Memory<ProductTable> GetProductsById(Memory<ProductTable> tables, int id) 
	{
		Span<ProductTable> currentTables = new ProductTable[tables.Length];
		int newProductsCount = 0;
		for (int i = 0; i < tables.Length; i++)
		{
			if (tables.Span[i].CategoryId == id) 
			{
				currentTables[newProductsCount] = tables.Span[i];
				newProductsCount++;
			}
        }
		return new Memory<ProductTable>(currentTables[0..newProductsCount].ToArray());
	}

	private IEnumerable<TLayout> GetTableElements<T, TElement, TLayout>(Memory<T> tables) where TLayout : Microsoft.Maui.ILayout, new() where TElement : ImmutableElement<T, TLayout>, new()
	{
        for (int i = 0; i < tables.Length; i++)
        {
			var customElement = new TElement();
			customElement.Inicialize(tables.Span[i]);
			yield return customElement.LayoutHandler;
        }
	}

	private bool TryUpdateCacheTables<T, TDirectory>(ref Memory<T> tables) where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
	{
		var oldTables = tables.Span.ToArray();
		using var cacheManager = new CacheHelper<T, TimeSpan, TDirectory>();
		var newTables = cacheManager.Deserialize();

		var keyCompare = CompareTimestamps((ICache<TimeSpan>[])(object)oldTables, (ICache<TimeSpan>[])(object)newTables);
		tables = newTables;
		return !(keyCompare);
	}

	private bool CompareTimestamps(ICache<TimeSpan>[] firstCaches, ICache<TimeSpan>[] secondCaches) 
	{
		if (firstCaches.Length != secondCaches.Length)
			return false;

        for (int i = 0; i < firstCaches.Length; i++)
        {
			if (firstCaches[i].Key.CompareTo(secondCaches[i].Key) == 1)
				return false;
        }
		return true;
	}
}
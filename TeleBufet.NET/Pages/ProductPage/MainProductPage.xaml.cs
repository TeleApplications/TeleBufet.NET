using DatagramsNet.Datagram;
using System.Text;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.ElementHelper;
using TeleBufet.NET.ElementHelper.Elements;

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
		Task.Run(async() => await UpdateElements());
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
		if (TryUpdateCacheTables<ProductTable, ProductCache>(ref products))
		{
			var cacheTable = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
			var oldProductData = cacheTable.Deserialize().ToArray();

			var requestProductInfromatios = new RequestProductInformationPacket() {ProductInformationTables = oldProductData};
			await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(requestProductInfromatios));
			var newProductData = cacheTable.Deserialize().ToArray();


			var productObjects = ProductElement.CreateElementObjects(products.Span.ToArray(), newProductData).ToArray();
			var elements = GetTableElements<ProductInformationHolder, ProductElement, StackLayout>(productObjects).ToArray();
			productFrames = new Frame[elements.Length];

            for (int i = 0; i < elements.Length; i++)
            {
				var baseFrame = new Frame() {CornerRadius = 15, Margin = 4, BackgroundColor = Colors.White };
				var baseHorizontalLayout = new StackLayout();

				var addButton = new Button() { BackgroundColor = Color.FromArgb("#4cb86b"), Padding = new Thickness(0, 0, 50, 45), VerticalOptions = LayoutOptions.End, HorizontalOptions = LayoutOptions.End, WidthRequest = 45, HeightRequest = 45, CornerRadius = 10};
				var currentProduct = products.Span[i];
				addButton.Clicked += async(object sender, EventArgs e) => 
				{
					await ProductManipulation(currentProduct, Operator.Plus); 
					Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Log", $"{LogCurrentCartCahce()}", "Close"));
				};
				addButton.IsEnabled = newProductData[i].Amount > 0;

                baseHorizontalLayout.Children.Add(elements[i]);
				baseHorizontalLayout.Children.Add(addButton);
				baseFrame.Content = baseHorizontalLayout;

				productFrames[i] = baseFrame;
            }
			SetCategory(null);
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

	private string LogCurrentCartCahce() 
	{
		string finalString = String.Empty;
		using var cartCacheHelper = new CartCacheHelper();
		var products = cartCacheHelper.Deserialize();

        for (int i = 0; i < products.Length; i++)
        {
			finalString += $"\n {products[i].Id}: {products[i].Amount}";
        }
		return finalString;
	}

	private async Task ProductManipulation(ProductTable table, Operator @operator)
	{
		using var cartCacheHelper = new CartCacheHelper();
		var holder = new ProductHolder(table.Id, 0);
		cartCacheHelper.CacheValue = holder;

		int currentAmount = cartCacheHelper.GetCurrentAmount();
		holder.Amount = currentAmount + (int)@operator;
		cartCacheHelper.CacheValue = holder;

		cartCacheHelper.Serialize();
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

	private async Task<bool> TryProductAmountAsync(ProductTable product)
	{
		var requestProductInfromatios = new RequestProductInformationPacket();
        await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(requestProductInfromatios));

		var cacheTable = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
        foreach (var table in cacheTable.Deserialize())
        {
			if (table.Id == product.Id)
				return table.Amount > 0;
        }
		return false;
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
		Span<T> oldTables = tables.Span;
		using var cacheManager = new CacheHelper<T, TimeSpan, TDirectory>();
		var newTables = cacheManager.Deserialize();

		tables = newTables;
		return tables.Span != oldTables;
	}
}
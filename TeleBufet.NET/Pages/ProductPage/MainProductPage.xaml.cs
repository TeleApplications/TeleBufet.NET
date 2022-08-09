using DatagramsNet.Datagram;
using System.Windows.Input;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.ElementHelper;
using TeleBufet.NET.ElementHelper.Elements;

namespace TeleBufet.NET.Pages.ProductPage;

public partial class MainProductPage : ContentPage
{
	private FlexLayout productLayout;
	private Memory<ProductTable> products = new();

	public static UserTable User { get; set; }

	public MainProductPage()
	{
		InitializeComponent();
		productLayout = collection;
		Task.Run(async() => await UpdateElements());
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


			var elements = GetTableElements<ProductInformationHolder, ProductElement, StackLayout>(ProductElementFactory.GetProductInformationTable(products.Span.ToArray(), newProductData).ToArray()).ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
				var baseFrame = new Frame() {CornerRadius = 15, Margin = 4, BackgroundColor = Colors.White };
				var baseHorizontalLayout = new StackLayout();

				var addButton = new Button() { BackgroundColor = Color.FromArgb("#4cb86b"), Padding = new Thickness(0, 0, 50, 45), VerticalOptions = LayoutOptions.End, HorizontalOptions = LayoutOptions.End, WidthRequest = 45, HeightRequest = 45, CornerRadius = 10 };
				addButton.IsEnabled = newProductData[i].Amount > 0;

                baseHorizontalLayout.Children.Add(elements[i]);
				baseHorizontalLayout.Children.Add(addButton);
				baseFrame.Content = baseHorizontalLayout;

				Device.BeginInvokeOnMainThread(() => productLayout.Children.Add(baseFrame));
            }
		}
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
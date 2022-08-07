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
		UpdateElements();
	}

	private async Task UpdateElements() 
	{
		if (TryUpdateCacheTables<ProductTable, ProductCache>(ref products))
		{
			//TODO: Create better dynamically adding products
			_ = await TryProductAmountAsync(new ProductTable());
			var cacheTable = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();

			var elementInformations = cacheTable.Deserialize().ToArray();
			var elements = GetTableElements<ProductInformationHolder, ProductElement, StackLayout>(ProductElementFactory.GetProductInformationTable(products.Span.ToArray(), elementInformations).ToArray()).ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
				var baseHorizontalLayout = new HorizontalStackLayout();
				var addButton = new Button() { BackgroundColor = Color.FromArgb("#4cb86b"), VerticalOptions = LayoutOptions.End, WidthRequest = 50, HeightRequest = 50, CornerRadius = 15 };
				addButton.IsEnabled = await TryProductAmountAsync(products.Span[i]);

				baseHorizontalLayout.Children.Add(new Frame() { Content = elements[i], CornerRadius = 15, Margin = 5, BackgroundColor = Colors.White });
				baseHorizontalLayout.Children.Add(addButton);
				productLayout.Children.Add(baseHorizontalLayout);
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
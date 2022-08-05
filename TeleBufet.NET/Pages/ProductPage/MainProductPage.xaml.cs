using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.ElementHelper;
using TeleBufet.NET.ElementHelper.Elements;

namespace TeleBufet.NET.Pages.ProductPage;

public partial class MainProductPage : ContentPage
{
	private Memory<ProductTable> products = new();

	public static UserTable User { get; set; }

	public MainProductPage()
	{
		InitializeComponent();
		Reload();
	}

	private void Reload() 
	{
		if (TryUpdateCacheTables<ProductTable, ProductCache>(ref products))
		{
			//TODO: Create better dynamically adding products
			var elements = GetTableElements<ProductTable, ProductElement, StackLayout>(products, new ProductElement()).ToArray();
            foreach (var element in elements)
            {
            }
		}
	}

	private IEnumerable<TLayout> GetTableElements<T, TElement, TLayout>(Memory<T> tables, TElement customElement) where T : ITable, ICache<TimeSpan> where TLayout : Microsoft.Maui.ILayout, new() where TElement : ImmutableElement<T, TLayout> 
	{
        for (int i = 0; i < tables.Length; i++)
        {
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
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.ElementHelper.Elements;

namespace TeleBufet.NET.Pages.ShoppingCartPage;

public partial class CartPage : ContentPage
{
	private StackLayout ordersLayout;

	public CartPage()
	{
		InitializeComponent();
		ordersLayout = orderLayout;
		using var cacheHelper = new CartCacheHelper();
		SetOrders(cacheHelper.Deserialize());
	}

	private void SetOrders(ProductHolder[] products) 
	{
        for (int i = 0; i < products.Length; i++)
        {
			var orderElement = new CartOrderElement();
			orderElement.Inicialize(products[i]);
			Device.BeginInvokeOnMainThread(() => ordersLayout.Children.Add(orderElement.LayoutHandler));
        }
	}
}
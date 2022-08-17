using DatagramsNet;
using DatagramsNet.Datagram;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
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
			var orderElement = new CartOrderElement(ordersLayout);
			orderElement.Inicialize(products[i]);
			Device.BeginInvokeOnMainThread(() => ordersLayout.Children.Add(orderElement.LayoutHandler));
        }
	}

	public async void ReservateProducts(object sender, EventArgs e) 
	{
		using var cartCacheHelper = new CartCacheHelper();
		var products = cartCacheHelper.Deserialize();
		double totalPrice = ComputeFinalPrice(products);

		var orderPacket = new OrderTransmitionPacket() 
		{
			Products = products,
			TotalPrice = totalPrice
		};

		await DatagramHelper.SendDatagramAsync(async (byte[] data) => await ExtendedClient.SendDataAsync(data), DatagramHelper.WriteDatagram(orderPacket));
	}

	private double ComputeFinalPrice(ProductHolder[] products) 
	{
		double finalPrice = 0;
		using var productInfromationCacheHelper = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
		var tables = productInfromationCacheHelper.Deserialize();

        for (int i = 0; i < products.Length; i++)
        {
			int index = products[i].Id;
			double currentPrice = tables[index].Price * products[i].Amount;

			finalPrice += currentPrice;
        }
		return finalPrice;
	}
}
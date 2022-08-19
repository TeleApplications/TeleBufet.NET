using DatagramsNet;
using DatagramsNet.Datagram;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.Immutable;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.API.Packets;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.ElementHelper.Elements;
using TeleBufet.NET.Pages.ProductPage;

namespace TeleBufet.NET.Pages.ShoppingCartPage;

public readonly struct Vector2 
{
	public double X { get; }
	public double Y { get; }

	public Vector2(double x, double y) 
	{
		X = x;
		Y = y;
	}
}

public partial class CartPage : ContentPage
{
	private StackLayout ordersLayout;
	private int currentBreak;


	private Ellipse breakIndicator = new Ellipse()
	{
		WidthRequest = 20,
		HeightRequest = 20,
		BackgroundColor = Color.FromArgb("#4cb86b"),
	};

	public CartPage()
	{
		InitializeComponent();
		ordersLayout = orderLayout;
		using var cacheHelper = new CartCacheHelper();
		SetOrders(cacheHelper.Deserialize());

		//TODO: Request server for specify count of breaks in the day
		SetSchoolBreaks(6);
	}

	private void SetSchoolBreaks(int breaks) 
	{
        for (int i = 0; i < breaks; i++)
        {
			var breakButton = new Button()
			{
				ZIndex = i,
				Text = $"{i}",
				TextColor = Colors.White,
				WidthRequest = 35,
				HeightRequest = 35,
				CornerRadius = 17, 
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 0, 10, 0),
				BackgroundColor = Color.FromRgb(0, 255 - (i * 20), (i * 5) + 10)
			};
			breakButton.Clicked += async (object sender, EventArgs e) =>
			{
				currentBreak = (sender as Button).ZIndex;
				await MoveIndicator(breakButton.WidthRequest, 15);
				await breakButton.ScaleTo(2);
				await breakButton.ScaleTo(1);
			};
			Device.BeginInvokeOnMainThread(() => breaksLayout.Children.Add(breakButton));
        }
		Device.BeginInvokeOnMainThread(() => indicatorLayout.Content = breakIndicator);
	}

	private async Task MoveIndicator(double width, int offset) 
	{
		var currentPosition = new Vector2(breakIndicator.TranslationX, breaksLayout.TranslationY);
		double destinationX = (((currentBreak) * width) + width / 2) + (offset * currentBreak);

		//await breakIndicator.TranslateTo(currentPosition.X + (index * 10), 0);
		await breakIndicator.TranslateTo(destinationX, currentPosition.Y);
	}

	private void SetOrders(ProductHolder[] products) 
	{
        for (int i = 0; i < products.Length; i++)
        {
			var orderElement = new CartOrderElement(ordersLayout);
			orderElement.Inicialize(products[i]);
			var baseFrame = new Frame()
			{
				BackgroundColor = Colors.White,
				HorizontalOptions = LayoutOptions.Fill,
				HeightRequest = 75,
				Margin = new Thickness(0, 0, 0, 15),
				CornerRadius = 15,
				Content = orderElement.LayoutHandler
			};

			Device.BeginInvokeOnMainThread(() => ordersLayout.Children.Add(baseFrame));
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
			TotalPrice = totalPrice,
			Indetifactor = MainProductPage.User.Id,
			ReservationTimeId = currentBreak
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
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;
using TeleBufet.NET.ElementHelper.Elements;

namespace TeleBufet.NET.Pages.TicketPage;

public partial class ReservationPage : ContentPage
{
	public ReservationPage()
	{
		InitializeComponent();
		using var ticketCacheManager = new CacheHelper<TicketHolder, TimeSpan, ReservationTicketCache>();
		var tables = ticketCacheManager.Deserialize();
		SetTickets(tables);
	}

	private void SetTickets(TicketHolder[] tickets) 
	{
        for (int i = 0; i < tickets.Length; i++)
        {
			var ticketElement = new TicketElement();
			ticketElement.Inicialize(tickets[i]);

			var baseFrame = new Frame()
			{
				HorizontalOptions = LayoutOptions.Fill,
				HeightRequest = 175,
				BackgroundColor = Colors.White,
				CornerRadius = 15,
				Margin = new Thickness(0, 0, 0, 25),
				Content = ticketElement.LayoutHandler
			};

			Device.BeginInvokeOnMainThread(() => ticketLayout.Children.Add(baseFrame));
        }
	}
}
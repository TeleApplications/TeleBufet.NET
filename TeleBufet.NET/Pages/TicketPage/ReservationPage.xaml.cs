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
		using var ticketCacheManager = new CacheHelper<TicketHolder>();
		var tables = ticketCacheManager.Deserialize();
		SetTickets(tables);
	}

	private void SetTickets(TicketHolder[] tickets)
	{
		var ticketSwipingElement = new SwipingElement();
		ticketSwipingElement.Inicialize(tickets);

		Device.BeginInvokeOnMainThread(() => ticketLayout.Children.Add(ticketSwipingElement.LayoutHandler));
	}
}
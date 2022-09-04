using TeleBufet.NET.CacheManager;
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

		var currentTickets = TicketHolder.GetCurrentTickets(tables);
		SetTickets(currentTickets);
	}

	private void SetTickets(ReadOnlyMemory<TicketHolder> tickets)
	{
		var ticketSwipingElement = new SwipingElement();
		ticketSwipingElement.Inicialize(tickets.ToArray());

		Device.BeginInvokeOnMainThread(() => ticketLayout.Children.Add(ticketSwipingElement.LayoutHandler));
	}
}
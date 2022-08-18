using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class TicketElement : ImmutableElement<TicketHolder, HorizontalStackLayout>
    {
        public override HorizontalStackLayout LayoutHandler { get; set; } = new HorizontalStackLayout()
        {
            HorizontalOptions = LayoutOptions.FillAndExpand,
            HeightRequest = 175,
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Label
            {
                FontSize = 24,
                FontFamily = "Consolas",
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center
            },
            new Label
            {
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            },
            new Label
            {
                FontSize = 18,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            }
        };

        public override void Inicialize(TicketHolder data)
        {
            (Controls.Span[0] as Label).Text = data.Id.ToString();
            (Controls.Span[1] as Label).Text = $"{data.Key} Přestávka";
            (Controls.Span[2] as Label).Text = $"{data.FinalPrice} Kč";
            base.Inicialize(data);
        }
    }
}

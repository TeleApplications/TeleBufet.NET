using Microsoft.Maui.Controls.Shapes;
using System.Text;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class TicketElement : ImmutableElement<TicketHolder, VerticalStackLayout>
    {
        private static readonly char dot = '.';

        public override VerticalStackLayout LayoutHandler { get; set; } = new VerticalStackLayout()
        {
            HeightRequest = 300,
            WidthRequest = 300 
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Label
            {
                FontSize = 20,
                FontFamily = "SourceCodePro",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                Text = "TeleBufet.NET",
                HorizontalOptions = LayoutOptions.Start
            },
            new Label
            {
                FontSize = 14,
                FontFamily = "SourceCodePro",
                TextColor = Colors.Black,
                Text = "Místo, kde tě C# obslouží",
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10,-5,0,0)
            },
            new Label
            {
                FontSize = 16,
                FontFamily = "SourceCodePro",
                TextColor = Colors.Black,
                Text = "Přestávka",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0,5,0,0)
            },
            new Label
            {
                FontSize = 16,
                FontFamily = "SourceCodePro",
                TextColor = Colors.Black,
                Text = "Cena",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0,5,0,0)
            },
            new Label
            {
                FontSize = 16,
                FontFamily = "SourceCodePro",
                TextColor = Colors.Black,
                Text = "Datum obědnávky",
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0,5,0,0)
            },
            new Label
            {
                FontSize = 16,
                FontFamily = "SourceCodePro",
                TextColor = Colors.Black,
                Text = "Status",
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0,5,0,0)
            },
        };

        public override void Inicialize(TicketHolder data)
        {
            int dotsLength = ((int)(LayoutHandler.WidthRequest)) / 15;

            var breakLabel = (Controls.Span[2] as Label);
            var priceLabel = (Controls.Span[3] as Label);
            var dateLabel = (Controls.Span[4] as Label);
            var statusLabel = (Controls.Span[5] as Label);

            breakLabel.Text += $"{CreateDots(dotsLength - breakLabel.Text.Length)}{data.Key}";
            priceLabel.Text += $"{CreateDots(dotsLength - priceLabel.Text.Length)}{data.FinalPrice} Kč";
            dateLabel.Text += $"{CreateDots(dotsLength - dateLabel.Text.Length)}{data.StringDateTime}";

            string stateText = data.IsExpired ? "Expired" : "Unexpired";
            statusLabel.Text += $"{CreateDots(dotsLength - statusLabel.Text.Length)}{stateText}";
            (Controls.Span[5] as Label).BackgroundColor = data.IsExpired ? Colors.Red : Colors.Green;
            base.Inicialize(data);
        }

        private string CreateDots(int length) 
        {
            var stringBuilder = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(dot);
            }

            return stringBuilder.ToString();
        }
    }
}

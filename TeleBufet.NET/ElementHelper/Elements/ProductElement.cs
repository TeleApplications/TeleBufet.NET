using System.Collections.Immutable;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.ElementHelper.Elements
{
    public sealed class ProductElement : ImmutableElement<ProductTable, StackLayout>
    {
        public override StackLayout LayoutHandler { get; } = new StackLayout()
        {
        };

        public int Category { get; private set; }

        public override ImmutableArray<View> Controls { get; protected set; }= ImmutableArray.Create<View>
        (
            new Image()
            {
                WidthRequest = 80,
                HeightRequest = 80,
            },
            new Label()
            {
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.Center
            },
            new Label()
            {
                FontSize = 18,
                TextColor = Color.FromArgb("#d08563")
            },
            new Label()
            {
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.End
            },
            new Button()
            {
                ImageSource = "add_icon.png",
                WidthRequest = 25,
                HeightRequest = 25,
                BackgroundColor = Color.FromArgb("#4cb86b"),
            }
        );

        public override void Inicialize(ProductTable data)
        {
            (Controls[0] as Image).Source = "microsoft_logo.png";
            (Controls[1] as Label).Text = data.Name;
            (Controls[2] as Label).Text = data.Price.ToString();
            (Controls[3] as Label).Text = data.Amount.ToString();

            Category = data.CategoryId;
            base.Inicialize(data);
        }
    }
}

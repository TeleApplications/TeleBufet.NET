using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.ElementHelper.Elements
{
    public readonly struct ProductInformationHolder 
    {
        public ProductTable Product { get; }
        public ProductInformationTable Information { get; }

        public ProductInformationHolder(ProductTable product, ProductInformationTable information) 
        {
            Product = product;
            Information = information;
        }
    }

    public static class ProductElementFactory 
    {
        public static IEnumerable<ProductInformationHolder> GetProductInformationTable(ProductTable[] table, ProductInformationTable[] information) 
        {
            for (int i = 0; i < table.Length; i++)
            {
                yield return new ProductInformationHolder(table[i], information[i]);
            }
        }
    }

    public sealed class ProductElement : ImmutableElement<ProductInformationHolder, StackLayout>
    {
        public int Category { get; private set; }
        public override StackLayout LayoutHandler { get; set; } = new StackLayout()
        {
            WidthRequest = 110,
            HeightRequest = 160,
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Image()
            {
                WidthRequest = 100,
                HeightRequest = 100,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center
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
                TextColor = Color.FromArgb("#4cb86b")
            },
            new Label()
            {
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalTextAlignment = TextAlignment.End
            },
        };

        public override void Inicialize(ProductInformationHolder data)
        {
            (Controls.Span[0] as Image).Source = "microsoft_logo.png";
            (Controls.Span[1] as Label).Text = data.Product.Name;
            (Controls.Span[2] as Label).Text = data.Information.Price.ToString();
            (Controls.Span[3] as Label).Text = data.Information.Amount.ToString();

            Category = data.Product.CategoryId;
            base.Inicialize(data);
        }
    }
}

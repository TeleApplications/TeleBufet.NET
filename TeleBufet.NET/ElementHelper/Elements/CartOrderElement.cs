using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class CartOrderElement : ImmutableElement<ProductHolder, HorizontalStackLayout>
    {
        private static ReadOnlyMemory<ProductTable> products;
        private static ReadOnlyMemory<ProductInformationTable> productsInformation;

        private Button[] manipulationButtons = new Button[]
        {
            new Button()
            {
                WidthRequest = 20,
                HeightRequest = 20,
                Text = "+",
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#4cb86b")
            },
            new Button()
            {
                WidthRequest = 20,
                HeightRequest = 20,
                Text = "-",
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = Color.FromArgb("#4cb86b")
            }
        };

        public CartOrderElement() 
        {
            using var productCacheHelper = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
            productsInformation = productCacheHelper.Deserialize();
        }

        public override HorizontalStackLayout LayoutHandler { get; set; } = new HorizontalStackLayout()
        {
            HeightRequest = 75,
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.White
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Image()
            {
                WidthRequest = 50,
                HeightRequest = 50,
                VerticalOptions = LayoutOptions.Start
            },
            new Label()
            {
                FontSize = 18,
                TextColor = Colors.Black,
            },
            new HorizontalStackLayout()
            {
                HeightRequest = 30,
                HorizontalOptions = LayoutOptions.Start
            }
        };

        public override void Inicialize(ProductHolder data)
        {
            var productTable = GetProductByTable(data);
            (Controls.Span[0] as Image).Source = "microsoft_logo.png";
            (Controls.Span[1] as Label).Text = productTable.Product.Name;

            Label priceText = new Label()
            {
                FontSize = 18,
                TextColor = Colors.LightGray,
                HorizontalOptions = LayoutOptions.Start,
                Text = $"{productTable.Information.Price} Kč"
            };

            Label amountPrice = new Label()
            {
                FontSize = 16,
                Text = data.Amount.ToString(),
                VerticalOptions = LayoutOptions.Start
            };

            (manipulationButtons[0]).Clicked += (object sender, EventArgs e) => ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Minus);
            (manipulationButtons[1]).Clicked += (object sender, EventArgs e) => ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Plus);
            var baseLayout = (Controls.Span[2] as HorizontalStackLayout);
            AddChildrens(baseLayout, priceText, manipulationButtons[0], amountPrice, manipulationButtons[1]);

            base.Inicialize(data);
        }

        private void AddChildrens(Microsoft.Maui.ILayout layout, params IView[] views) 
        {
            for (int i = 0; i < views.Length; i++)
            {
                layout.Add(views[i]);
            }
        }

        private ProductInformationHolder GetProductByTable<T>(T tables) where T : ITable
        {
            if (products.IsEmpty)
            {
                using var productCacheHelper = new TableCacheHelper<ProductTable, ProductCache>();
                products = productCacheHelper.Deserialize();
            }

            for (int i = 0; i < products.Length; i++)
            {
                var product = products.Span[i];
                if (product.Id == tables.Id)
                    return new ProductInformationHolder(product, productsInformation.Span[i]);
            }
            return default;
        }
    }
}

using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class CartOrderElement : ImmutableElement<ProductHolder, StackLayout>
    {
        private static ReadOnlyMemory<ProductTable> products;
        private static ReadOnlyMemory<ProductInformationTable> productsInformation;

        public CartOrderElement() 
        {
            using var productCacheHelper = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
            productsInformation = productCacheHelper.Deserialize();
        }

        public override StackLayout LayoutHandler { get; set; } = new StackLayout()
        {
            WidthRequest = 175,
            HeightRequest = 75,
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Image()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center
            },
            new Label()
            {
                FontSize = 24,
                TextColor = Colors.Black,
            },
            new Button()
            {
                WidthRequest = 20,
                HeightRequest = 20,
                Text = "+",
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#4cb86b")
            },
            new Button()
            {
                WidthRequest = 20,
                HeightRequest = 20,
                Text = "-",
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#4cb86b")
            },
            new Label()
            {
                FontSize = 28,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.End
            },
        };

        public override void Inicialize(ProductHolder data)
        {

            var productTable = GetProductByTable(data);
            (Controls.Span[0] as Image).Source = "microsoft_logo.png";
            (Controls.Span[1] as Label).Text = productTable.Product.Name;
            (Controls.Span[2] as Button).Clicked += (object sender, EventArgs e) => ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Plus);
            (Controls.Span[3] as Button).Clicked += (object sender, EventArgs e) => ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Minus);
            (Controls.Span[4] as Label).Text = productTable.Information.Price.ToString();
            base.Inicialize(data);
        }

        private ProductInformationHolder GetProductByTable<T>(T tables) where T : ITable
        {
            if (products.Span == Array.Empty<ProductTable>()) 
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

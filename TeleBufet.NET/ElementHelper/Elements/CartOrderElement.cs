using DatagramsNet;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager;
using TeleBufet.NET.CacheManager.CacheDirectories;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class CartOrderElement : UpdateElement<ProductHolder, HorizontalStackLayout>
    {
        private static ReadOnlyMemory<ProductTable> products;
        private static ReadOnlyMemory<ProductInformationTable> productsInformation;

        //We do it in that way because the refresh of whole site for just only
        //updating the amount will cause big performace issue
        //In next update in MAUI, this will be changed
        private ProductHolder currentProduct;

        public StackLayout MainLayout { get; }
        public double ProductPrice { get; private set; }

        public CartOrderElement(StackLayout stackLayout)
        {
            MainLayout = stackLayout;

            using var productCacheHelper = new TableCacheHelper<ProductInformationTable, ProductInformationCache>();
            productsInformation = productCacheHelper.Deserialize();
        }

        public override HorizontalStackLayout LayoutHandler { get; set; } = new HorizontalStackLayout()
        {
            HeightRequest = 55,
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Transparent
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Image()
            {
                WidthRequest = 65,
                HeightRequest = 65,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            },
            new Label()
            {
                FontSize = 16,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(15,0,25,0)
            },
            new Button()
            {
                WidthRequest = 25,
                HeightRequest = 25,
                Text = "-",
                FontSize = 10,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#4cb86b"),
                Margin = new Thickness(0,0,10,0)
            },

            new Label()
            {
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0,0,10,0)
            },

            new Button()
            {
                WidthRequest = 25,
                HeightRequest = 25,
                Text = "+",
                FontSize = 10,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#4cb86b"),
            },
            new Label()
            {
                FontSize = 16,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = Colors.Black,
            },
        };

        public override void Inicialize(ProductHolder data)
        {
            var productTable = GetProductByTable(data);
            currentProduct = data;
            ProductPrice = productTable.Information.Price;

            (Controls.Span[0] as Image).Source = "microsoft_logo.png";
            (Controls.Span[1] as Label).Text = productTable.Product.Name;

            (Controls.Span[2] as Button).Clicked += (object sender, EventArgs e) => 
            {
                _ = ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Minus);
                _ = UpdateAsync();
            };
            (Controls.Span[3] as Label).Text = data.Amount.ToString();
            (Controls.Span[4] as Button).Clicked += (object sender, EventArgs e) => 
            {
                _ = ProductElement.ProductManipulation(productTable.Product, Pages.ProductPage.Operator.Plus);
                _ = UpdateAsync();
            };

            (Controls.Span[5] as Label).Text = productTable.Information.Price.ToString();
            base.Inicialize(data);
        }

        protected override async Task UpdateAsync()
        {
            using var cartCacheHelper = new CartCacheHelper();
            cartCacheHelper.CacheValue = currentProduct;

            int finalAmount = cartCacheHelper.GetCurrentAmount();
            if (finalAmount != 0)
            {
                double finalPrice = ProductPrice * finalAmount;

                (Controls.Span[3] as Label).Text = finalAmount.ToString();
                (Controls.Span[5] as Label).Text = finalPrice.ToString();
            }
            else
                MainLayout.Remove(LayoutHandler);
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

using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ShoppingCartCache;
using TeleBufet.NET.ElementHelper.Interfaces;
using TeleBufet.NET.Pages.ProductPage;

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

    public sealed class ProductElement : ImmutableElement<ProductInformationHolder, StackLayout>, IElementTypeFactory<ProductInformationHolder, ProductTable, ProductInformationTable[]>
    {
        public int Category { get; private set; }
        public override StackLayout LayoutHandler { get; set; } = new StackLayout()
        {
            WidthRequest = 110,
            HeightRequest = 160,
        };

        public static IEnumerable<ProductInformationHolder> CreateElementObjects(ProductTable[] objectOne, ProductInformationTable[] objectTwo) 
        {
            for (int i = 0; i < objectOne.Length; i++)
            {
                yield return new ProductInformationHolder(objectOne[i], objectTwo[i]);
            }
        }


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

	    public static async Task ProductManipulation(ProductTable table, Operator @operator)
	    {
		    using var cartCacheHelper = new CartCacheHelper();
		    var holder = new ProductHolder(table.Id, 0);
		    cartCacheHelper.CacheValue = holder;

		    int currentAmount = cartCacheHelper.GetCurrentAmount();
		    holder.Amount = currentAmount + (int)@operator;
		    cartCacheHelper.CacheValue = holder;

		    cartCacheHelper.Serialize();
	    }
    }
}

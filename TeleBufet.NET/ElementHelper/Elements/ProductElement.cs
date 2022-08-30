using DatagramsNet;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.CacheManager.CacheDirectories;
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

    internal sealed class ProductElement : UpdateElement<ProductInformationHolder, StackLayout>, IElementTypeFactory<ProductInformationHolder, ProductTable, ProductInformationTable[]>
    {
        public int Category { get; private set; }

        private ProductInformationTable UpdateHolder { get; set; }

        public override StackLayout LayoutHandler { get; set; } = new StackLayout()
        {
            WidthRequest = 110,
        };

        public static ReadOnlyMemory<ProductInformationHolder> CreateElementObjects(ProductTable[] objectOne, ProductInformationTable[] objectTwo) 
        {
            Memory<ProductInformationHolder> holder = new ProductInformationHolder[objectOne.Length];
            for (int i = 0; i < objectOne.Length; i++)
            {
                holder.Span[i] = new ProductInformationHolder(objectOne[i], objectTwo[i]);
            }
            return holder;
        }

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Image()
            {
                WidthRequest = 100,
                HeightRequest = 100,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                Aspect = Aspect.AspectFit
            },
            new Label()
            {
                FontSize = 14,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center
            },
            new Label()
            {
                FontSize = 12,
                TextColor = Color.FromHex("#A7ABB2"),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
            },
            new Label()
            {
                FontSize = 18,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
                VerticalTextAlignment = TextAlignment.End,
                VerticalOptions = LayoutOptions.End,
                Margin = new Thickness(0,0,10,0),
            },
            new Button()
            {
                HorizontalOptions = LayoutOptions.End,
                BackgroundColor = Color.FromArgb("#25A914"),
                Padding = new Thickness(0, 0, 50, 45),
                VerticalOptions = LayoutOptions.End,
                WidthRequest = 45,
                HeightRequest = 45,
                CornerRadius = 10
            },
        };

        public override void Inicialize(ProductInformationHolder data)
        {
            var currentImage = ImageCache.GetCacheTables().ToArray().FirstOrDefault(n => n.Id == data.Product.ImageId);
            currentImage = currentImage ?? new ImageTable() { Source = "microsoft_logo.png" };

            (Controls.Span[0] as Image).Source = currentImage.Source;
            (Controls.Span[1] as Label).Text = data.Product.Name;

            var currentButton = (Controls.Span[4] as Button);
            var currentProduct = data.Product;
			currentButton.Clicked += async(object sender, EventArgs e) => 
			{
				await ProductManipulation(currentProduct, Operator.Plus); 
			};

            Category = data.Product.CategoryId;
            UpdateHolder = data.Information;

            _ = UpdateAsync();
            base.Inicialize(data);
        }

        protected override async Task UpdateAsync()
        {
            var priceLabel = (Controls.Span[3] as Label);
            priceLabel.Text = $"{UpdateHolder.Price} Kč";

            var amountLabel = (Controls.Span[2] as Label);
            var addButton = (Controls.Span[4] as Button);

            Memory<View> currentViews = new View[] { priceLabel, addButton };
            bool amountCondition = UpdateHolder.Amount > 0;

            SetViewsState(currentViews, (View view, bool isEnable) => view.IsVisible = isEnable, amountCondition);
            if (amountCondition)
            {
                amountLabel.Text = $"{UpdateHolder.Amount} skladem";
                amountLabel.BackgroundColor = Colors.Transparent;
                amountLabel.TextColor = Color.FromHex("#A7ABB2");

            }
            else 
            {
                amountLabel.Text = "Vyprodáno";
                amountLabel.BackgroundColor = Colors.Red;
                amountLabel.TextColor = Colors.White;
            }
        }

        public static bool TryUpdateElement(ref ProductElement element, ProductInformationTable holder) 
        {
            if (element.UpdateHolder.Amount == holder.Amount && element.UpdateHolder.Price == holder.Price)
                return false;
            element.UpdateHolder = holder;
            _ = element.UpdateAsync();

            return true;
        }

	    public static async Task ProductManipulation(ProductTable table, Operator @operator)
	    {
		    using var oldCartCacheHelper = new CartCacheHelper();
		    var holder = new ProductHolder(table.Id, 0);
		    oldCartCacheHelper.CacheValue = holder;
		    int currentAmount = oldCartCacheHelper.GetCurrentAmount();

		    using var newCartCacheHelper = new CartCacheHelper();
		    holder.Amount = currentAmount + (int)@operator;
		    newCartCacheHelper.CacheValue = holder;
		    newCartCacheHelper.Serialize();
	    }

        private void SetViewsState(Memory<View> views, Action<View, bool> viewAction, bool isEnable) 
        {
            for (int i = 0; i < views.Length; i++)
            {
                viewAction.Invoke(views.Span[i], isEnable);
            }
        }
    }
}

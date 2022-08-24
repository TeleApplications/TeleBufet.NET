using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Database.Tables;
using TeleBufet.NET.ElementHelper.Interfaces;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal readonly struct ActionTable<T> where T : ITable
    {
        public T Table { get; }
        public Action<T> TableAction { get; }

        public ActionTable(T table, Action<T>? tableAction) 
        {
            Table = table;
            TableAction = tableAction ?? new Action<T>((T table) => Device.BeginInvokeOnMainThread(async() => await App.Current.MainPage.DisplayAlert("Warning", "This action is not implemented", "Close")));
        }
    }

    internal sealed class CategoryElement : ImmutableElement<ActionTable<CategoryTable>, Grid>, IElementTypeFactory<ActionTable<CategoryTable>, CategoryTable, Action<CategoryTable>>
    {
        public override Grid LayoutHandler { get; set; } = new Grid()
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
            HorizontalOptions = LayoutOptions.Center,
            HeightRequest = 35 
        };

        public static ReadOnlyMemory<ActionTable<CategoryTable>> CreateElementObjects(CategoryTable[] objectOne, Action<CategoryTable> objectTwo) 
        {
            Memory<ActionTable<CategoryTable>> holder = new ActionTable<CategoryTable>[objectOne.Length];
            for (int i = 0; i < objectOne.Length; i++)
            {
                holder.Span[i] = new ActionTable<CategoryTable>(objectOne[i], objectTwo);
            }
            return holder;
        }

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new Button()
            {
                FontSize = 16,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold,
                HeightRequest = 40,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.StartAndExpand
            }
        };

        public override void Inicialize(ActionTable<CategoryTable> data)
        {
            (Controls.Span[0] as Button).Text = data.Table.Name;
            (Controls.Span[0] as Button).Clicked += (object sender, EventArgs e) => { data.TableAction.Invoke(data.Table); };
            base.Inicialize(data);
        }

    }
}

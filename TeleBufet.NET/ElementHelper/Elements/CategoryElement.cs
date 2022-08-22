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
            HorizontalOptions = LayoutOptions.CenterAndExpand,
            WidthRequest = 45,
            HeightRequest = 10,
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
                BackgroundColor = Colors.Transparent,
                HeightRequest = 10,
                FontSize = 12,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.FillAndExpand
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

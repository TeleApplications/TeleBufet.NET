using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            WidthRequest = 35,
            HeightRequest = 45,
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
            new ImageButton()
            {
                WidthRequest = 35,
                HeightRequest = 35 
            }
        };

        public override void Inicialize(ActionTable<CategoryTable> data)
        {
            (Controls.Span[0] as ImageButton).Source = "microsoft_logo.png";
            (Controls.Span[0] as ImageButton).Clicked += (object sender, EventArgs e) => { data.TableAction.Invoke(data.Table); };
            base.Inicialize(data);
        }

    }
}

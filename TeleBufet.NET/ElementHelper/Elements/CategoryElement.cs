using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleBufet.NET.API.Database.Tables;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class CategoryElement : ImmutableElement<CategoryTable, Grid>
    {
        public override Grid LayoutHandler { get; set; } = new Grid()
        {
            WidthRequest = 35,
            HeightRequest = 45,
        };

        public override Memory<View> Controls { get; protected set; } = new View[]
        {
            new ImageButton()
            {
                WidthRequest = 35,
                HeightRequest = 35 
            }
        };

        public override void Inicialize(CategoryTable data)
        {
            (Controls.Span[0] as ImageButton).Source = "microsoft_logo.png";
            base.Inicialize(data);
        }
    }
}

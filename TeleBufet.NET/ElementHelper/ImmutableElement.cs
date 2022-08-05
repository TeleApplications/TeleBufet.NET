using System.Collections.Immutable;
using TeleBufet.NET.ElementHelper.Interfaces;

namespace TeleBufet.NET.ElementHelper
{
    public abstract class ImmutableElement<T, TLayout> : ICustomElement<TLayout> where TLayout : Microsoft.Maui.ILayout, new()
    {
        public virtual TLayout LayoutHandler { get; } = new();
        public abstract ImmutableArray<View> Controls { get; protected set; }

        public virtual void Inicialize(T data) 
        {
            for (int i = 0; i < Controls.Length; i++)
            {
                LayoutHandler.Add(Controls[i]);
            }
        }
    }
}


namespace TeleBufet.NET.ElementHelper.Interfaces
{
    internal interface ICustomElement<TLayout> where TLayout : Microsoft.Maui.ILayout
    {
        public TLayout LayoutHandler { get; }

        public virtual void Inicialize() { }
    }
}

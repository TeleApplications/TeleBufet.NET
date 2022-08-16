
namespace TeleBufet.NET.ElementHelper
{
    internal abstract class UpdateElement<T, TLayout> : ImmutableElement<T, TLayout> where TLayout : Microsoft.Maui.ILayout, new()
    {
        public override Memory<View> Controls { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override TLayout LayoutHandler { get; set; }

        public UpdateElement() 
        {
            Task.Run(async () => await UpdateAsync());
        }

        protected virtual async Task UpdateAsync() { await Task.CompletedTask; }
    }
}

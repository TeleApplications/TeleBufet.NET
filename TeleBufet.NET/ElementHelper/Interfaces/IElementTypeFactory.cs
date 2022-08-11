
namespace TeleBufet.NET.ElementHelper.Interfaces
{
    internal interface IElementTypeFactory<TResult, T1, T2>
    {
        public static IEnumerable<TResult> CreateElementObjects(T1[] objectOne, T2 objectTwo) { return null; }
    }
}

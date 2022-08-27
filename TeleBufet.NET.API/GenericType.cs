
namespace TeleBufet.NET.API
{
    public static class GenericType
    {
        public static T ReType<T>(object @object) => (T)@object;
    }
}

using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class CacheTableAttribute : Attribute
    {
        public Type TableType { get; }

        public CacheTableAttribute(Type tableType) 
        {
            if (tableType.IsDefined(typeof(ICacheTable<TimeSpan>), true))
                throw new Exception("Table must derived from ICache for caching usage");
            TableType = tableType;
        }
    }
}

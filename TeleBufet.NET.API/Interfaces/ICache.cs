using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Interfaces
{
    public interface ICacheTable<T> : ITable
    {
        public T Key { get; set; }
    }
}

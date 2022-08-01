using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager
{
    internal sealed class TableCacheHelper<T,TDirectory> : CacheHelper<T, TimeSpan, TDirectory> where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new()
    {
        protected override void SetBinarySeek()
        {
            var cacheTables = Deserialize();
            int index = GetTableIndex(CacheValue, cacheTables);
            directory.CacheFileStream.Seek(index, SeekOrigin.Begin);
        }

        private int GetTableIndex(T currentTable, T[] cacheTables) 
        {
            for (int i = 0; i < cacheTables.Length; i++)
            {
                if (cacheTables[i].Id == currentTable.Id)
                    return i * ValueSize;
            }
            return (cacheTables.Length) * ValueSize;
        }
    }
}

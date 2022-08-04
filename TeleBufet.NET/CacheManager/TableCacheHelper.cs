using DatagramsNet;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager
{
    internal sealed class TableCacheHelper<T,TDirectory> : CacheHelper<T, TimeSpan, TDirectory> where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new()
    {
        private static byte[] bytesHolder = new byte[1];
        protected override void SetBinarySeek()
        {
            var cacheTables = Deserialize();
            int index = GetTableIndex(CacheValue, cacheTables);
            var origin = index == 0 ? SeekOrigin.End : SeekOrigin.Begin;
            directory.CacheFileStream.Seek(index, origin);
        }

        private int GetTableIndex(T currentTable, T[] cacheTables) 
        {
            for (int i = 0; i < cacheTables.Length; i++)
            {
                if (cacheTables[i].Id == currentTable.Id) 
                {
                    int tableSize = BinaryHelper.GetSizeOf(cacheTables[i], typeof(T), ref bytesHolder);
                    return i * tableSize;
                }
            }
            return 0;
        }
    }
}

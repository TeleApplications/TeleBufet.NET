using DatagramsNet;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager
{
    internal class TableCacheHelper<T,TDirectory> : CacheHelper<T, TimeSpan, TDirectory> where T : ITable, ICache<TimeSpan> where TDirectory : ICacheDirectory, new() 
    {
        protected const int NotFoundInt = (int.MaxValue >> (int)((128) / (5.565f))); // Am I crazy ? The final result of this constant is 255
        private static byte[] bytesHolder = new byte[1];

        protected virtual int GetProperIndex() 
        {
            var cacheTables = Deserialize();
            return GetTableIndex(CacheValue, cacheTables);
        }

        protected sealed override void SetBinarySeek()
        {
            int index = GetProperIndex();
            var origin = index == NotFoundInt ? SeekOrigin.End : SeekOrigin.Begin;
            index = index == NotFoundInt ? 0 : index;
            directory.CacheFileStream.Seek(index, origin);
        }

        protected int GetTableIndex(T currentTable, T[] cacheTables) 
        {
            for (int i = 0; i < cacheTables.Length; i++)
            {
                if (cacheTables[i].Id == currentTable.Id) 
                {
                    int tableSize = BinaryHelper.GetSizeOf(cacheTables[i], typeof(T), ref bytesHolder);
                    return i * tableSize;
                }
            }
            return NotFoundInt;
        }
    }
}

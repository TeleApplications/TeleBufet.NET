using DatagramsNet;
using System.Reflection;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.API.Packets.ClientSide;

namespace TeleBufet.NET.CacheManager
{
    internal static class TableCacheBuilder
    {
        private static TableCacheHelper<T> CreateInstance<T>() where T : ICacheTable<TimeSpan> => new TableCacheHelper<T>();

        public static MethodInfo CreateGenericInstance = typeof(TableCacheBuilder).GetMethod(nameof(TableCacheBuilder.CacheTables));

        public static void CacheTables<T>(ReadOnlyMemory<T> tables) where T : ICacheTable<TimeSpan>
        {
            for (int i = 0; i < tables.Length; i++)
            {
                using var cacheManager = new TableCacheHelper<T>(tables.Span[i]);
                cacheManager.Serialize();
            }
        }

        public static IEnumerable<CacheConnection> GetCacheConnectionKeys<T>() where T : ICacheTable<TimeSpan>
        {
            using var cacheManager = new TableCacheHelper<T>();
            var tables = cacheManager.Deserialize();
            for (int i = 0; i < tables.Length; i++)
            {
                var connectionKey = new CacheConnection(tables[i]);
                yield return connectionKey;
            }
        }

    }

    internal class TableCacheHelper<T> : CacheHelper<T> where T : ICacheTable<TimeSpan>
    {
        protected const int NotFoundInt = (int.MaxValue >> (int)((128) / (5.565f))); // Am I crazy ? The final result of this constant is 255
        private static byte[] bytesHolder = new byte[1];

        public TableCacheHelper() { }
        public TableCacheHelper(T value) : base(value) { }

        protected virtual int GetProperIndex() 
        {
            var cacheTables = Deserialize();
            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();
            return GetTableIndex(cacheTables);
        }

        protected sealed override void SetBinarySeek()
        {
            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();
            int index = GetProperIndex();
            var origin = index == NotFoundInt ? SeekOrigin.End : SeekOrigin.Begin;
            index = index == NotFoundInt ? 0 : index;
            directory.CacheFileStream.Seek(index, origin);
        }

        protected int GetTableIndex(T[] cacheTables) 
        {
            if (CacheValue is null)
                return NotFoundInt;
            for (int i = 0; i < cacheTables.Length; i++)
            {
                if (cacheTables[i].Id == CacheValue.Id) 
                {
                    int tableSize = BinaryHelper.GetSizeOf(cacheTables[i], typeof(ICacheTable<TimeSpan>), ref bytesHolder);
                    return i * tableSize;
                }
            }
            return NotFoundInt;
        }
    }
}

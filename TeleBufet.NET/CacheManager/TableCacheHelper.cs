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

        public static ReadOnlyMemory<CacheConnection> GetCacheConnectionKeys<T>() where T : ICacheTable<TimeSpan>
        {
            using var cacheManager = new TableCacheHelper<T>();
            var tables = cacheManager.Deserialize();

            Memory<CacheConnection> connectionKeys = new CacheConnection[tables.Length];
            for (int i = 0; i < tables.Length; i++)
            {
                var connectionKey = new CacheConnection(tables[i], tables[i].Key);
                connectionKeys.Span[i] = connectionKey;
            }
            return connectionKeys;
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

            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();
            directory.CacheFileStream.Seek(index, origin);
        }

        protected int GetTableIndex(T[] cacheTables) 
        {
            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();

            Span<T> spanTables = cacheTables;
            int directoryLength = ((int)(directory.CacheFileStream.Length));

            if (CacheValue is null)
                return NotFoundInt;
            for (int i = 0; i < cacheTables.Length; i++)
            {
                if (cacheTables[i].Id == CacheValue.Id) 
                {
                    //This part where we add multiplied sizeof is really temporary solution
                    //due to unupdated version of Datagrams.NET where getting serialized size of object
                    //is possible
                    var finalSize = BinaryHelper.GetSizeOfArray(spanTables[0..(i)].ToArray(), ref bytesHolder);
                    finalSize = finalSize + (i * sizeof(int));

                    var oldSize = BinaryHelper.GetSizeOf(cacheTables[i], typeof(T), ref bytesHolder) + sizeof(int);
                    var newSize = BinaryHelper.GetSizeOf(CacheValue, typeof(T), ref bytesHolder) + sizeof(int);

                    int difference = newSize - oldSize;
                    directory.CacheFileStream.SetLength(directoryLength + difference);

                    if (i > (cacheTables.Length - 1) && difference != 0) 
                    {
                        int shiftBytesLength = directoryLength - (finalSize + oldSize);
                        var shiftBytes = GetShiftBytes(shiftBytesLength, 0).Span;

                        directory.CacheFileStream.Seek(finalSize + newSize, SeekOrigin.Begin);
                        using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
                        binaryWriter.Write(shiftBytes);
                    }
                        using var binaryReader = new BinaryReader(directory.CacheFileStream);
                    var bytes = binaryReader.ReadBytes(directoryLength);

                    return finalSize;
                }
            }
            return NotFoundInt;
        }
    }
}

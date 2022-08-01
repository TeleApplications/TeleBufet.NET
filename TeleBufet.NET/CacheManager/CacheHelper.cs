using System.Runtime.InteropServices;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.API.Interfaces;
using DatagramsNet;

namespace TeleBufet.NET.CacheManager
{
    //TODO: This implementation is not complete 
    internal class CacheHelper<T, TKey, TDirectory> : IDisposable where T : ICache<TKey> where TDirectory : ICacheDirectory, new()
    {
        public T CacheValue { get; set; }

        protected TDirectory directory = new TDirectory();

        protected readonly int ValueSize = Marshal.SizeOf<T>();

        public CacheHelper() { }

        public CacheHelper(T value) 
        {
            CacheValue = value;
        }

        protected virtual void SetBinarySeek() => directory.CacheFileStream.Seek(0, SeekOrigin.End);

        public void Serialize() 
        {
            SetBinarySeek();
            using (var binaryWriter = new BinaryWriter(directory.CacheFileStream)) 
            {
                byte[] bytes = BinaryHelper.Write(CacheValue);
                binaryWriter.Write(bytes);
            }
        }

        public T[] Deserialize() 
        {
            directory.CacheFileStream.Seek(0, SeekOrigin.Begin);

            int size = ValueSize;
            using var binaryReader = new BinaryReader(directory.CacheFileStream);

            int count = (int)binaryReader.BaseStream.Length / size;
            var values = new T[count];
            Span<byte> spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length).AsSpan();
            for (int i = 0; i < count; i++)
            {
                int byteCount = ((i + 1) * size);
                byte[] newBytes = spanBytes.Slice((i * size), size).ToArray();
                T @object = BinaryHelper.Read<T>(newBytes);
                values[i] = @object;
            }
            return values;
        }

        public void Dispose() 
        {
            directory.CacheFileStream.Dispose();
        }
    }
}

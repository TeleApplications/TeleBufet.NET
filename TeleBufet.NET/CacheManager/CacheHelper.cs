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

        public CacheHelper() { }

        public CacheHelper(T value) 
        {
            CacheValue = value;
        }

        protected virtual void SetBinarySeek() => directory.CacheFileStream.Seek(0, SeekOrigin.End);

        public virtual void Serialize() 
        {
            SetBinarySeek();
            var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            byte[] bytes = BinaryHelper.Write(CacheValue);
            binaryWriter.Write(bytes);
        }

        public virtual T[] Deserialize() 
        {
            directory.CacheFileStream.Seek(0, SeekOrigin.Begin);

            var binaryReader = new BinaryReader(directory.CacheFileStream);
            Span<byte> spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length).AsSpan();
            return BinaryHelper.Read<T[]>(spanBytes.ToArray());
        }

        public void Dispose() 
        {
            directory.CacheFileStream.Flush();
            directory.CacheFileStream.Dispose();
        }
    }
}

using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.API.Interfaces;
using DatagramsNet;
using TeleBufet.NET.CacheManager.CacheDirectories;

namespace TeleBufet.NET.CacheManager
{
    internal class CacheHelper<T, TKey, TDirectory> : IDisposable where T : ICache<TKey> where TDirectory : ICacheDirectory, new()
    {
        public T CacheValue { get; set; }

        protected TDirectory directory = new TDirectory();

        public CacheHelper() { }

        public CacheHelper(T value) 
        {
            CacheValue = value;
            if (!directory.CacheFileStream.CanRead)
                directory = new();
        }

        protected virtual void SetBinarySeek() => directory.CacheFileStream.Seek(0, SeekOrigin.End);

        public virtual void Serialize() 
        {
            SetBinarySeek();
            using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            byte[] bytes = BinaryHelper.Write(CacheValue);
            binaryWriter.Write(bytes);
        }

        public virtual T[] Deserialize() 
        {
            directory.CacheFileStream.Seek(0, SeekOrigin.Begin);

            using var binaryReader = new BinaryReader(directory.CacheFileStream);
            Span<byte> spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length).AsSpan();
            return BinaryHelper.Read<T[]>(spanBytes.ToArray());
        }

        public void RemoveBytes(int startIndex, int length) 
        {
            var shiftBytes = GetShiftBytes(startIndex, length).Span;

            if (!directory.CacheFileStream.CanRead)
                directory = new();
            directory.CacheFileStream.Seek(startIndex, SeekOrigin.Begin);
            using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            int fileLength = (int)directory.CacheFileStream.Length;
            binaryWriter.Write(shiftBytes);

            directory.CacheFileStream.SetLength(fileLength - length);
        }

        private Memory<byte> GetShiftBytes(int startIndex, int length) 
        {
            directory.CacheFileStream.Seek(startIndex, SeekOrigin.Begin);
            using var binaryReader = new BinaryReader(directory.CacheFileStream);
            var spanBytes = binaryReader.ReadBytes((int)directory.CacheFileStream.Length);
            return spanBytes[(length)..];
        }

        public void Dispose() 
        {
            directory.CacheFileStream.Close();
        }
    }
}

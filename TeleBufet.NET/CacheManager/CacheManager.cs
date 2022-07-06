using System.Runtime.InteropServices;
using TeleBufet.NET.API.Interfaces;
using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager
{
    //TODO: This implementation is not complete 
    internal sealed class CacheHelper<T, TKey, TDirectory> : IDisposable where T : struct, ICache<TKey> where TDirectory : ICacheDirectory, new()
    {
        private T cacheValue;

        private TDirectory directory = new TDirectory();

        public CacheHelper(T value) 
        {
            cacheValue = value;
        }

        public void Serialize() 
        {
            using (var binaryWriter = new BinaryWriter(directory.CacheFileStream)) 
            {
                byte[] bytes = GetBytes(cacheValue);
                binaryWriter.Write(bytes);
            }
        }

        public IEnumerable<T> Deserialize() 
        {
            int size = Marshal.SizeOf<T>(cacheValue);
            using (var binaryReader = new BinaryReader(directory.CacheFileStream))
            {
                int count = (int)binaryReader.BaseStream.Length / size;
                for (int i = 0; i < size; i++)
                {
                    int byteCount = ((i + 1) * size);
                    var bytes = binaryReader.ReadBytes(byteCount).AsSpan();
                    T @object = GetT(bytes.Slice((i * size), byteCount).ToArray());
                    yield return @object;
                }
            }
        }

        private byte[] GetBytes(T value)
        {
            int size = Marshal.SizeOf<T>(cacheValue);
            byte[] structureBytes = new byte[size];
            IntPtr valuePointer = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(value, valuePointer, false);
            Marshal.Copy(valuePointer, structureBytes, 0, size);
            return structureBytes;
        }

        private T GetT(byte[] bytes) 
        {
            ReadOnlySpan<byte> spanBytes = new ReadOnlySpan<byte>(bytes);
            return MemoryMarshal.Read<T>(spanBytes);
        }

        public void Dispose() 
        {
            directory.CacheFileStream.Dispose();
            this.Dispose();
        }
    }
}

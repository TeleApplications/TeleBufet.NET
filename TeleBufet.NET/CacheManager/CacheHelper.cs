using System.Runtime.InteropServices;
using TeleBufet.NET.CacheManager.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.CacheManager
{
    //TODO: This implementation is not complete 
    internal sealed class CacheHelper<T, TKey, TDirectory> : IDisposable where T : ICache<TKey> where TDirectory : ICacheDirectory, new()
    {
        private T cacheValue;

        private TDirectory directory = new TDirectory();

        private readonly int ValueSize = Marshal.SizeOf<T>();

        public CacheHelper() { }

        public CacheHelper(T value) 
        {
            cacheValue = value;
        }

        public void Serialize() 
        {
            directory.CacheFileStream.Seek(0, SeekOrigin.End);
            using (var binaryWriter = new BinaryWriter(directory.CacheFileStream)) 
            {
                byte[] bytes = GetBytes(cacheValue);
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
                T @object = GetT(newBytes);
                values[i] = @object;
            }
            return values;
        }

        private byte[] GetBytes(T value)
        {
            byte[] structureBytes = new byte[ValueSize];
            IntPtr valuePointer = Marshal.AllocHGlobal(ValueSize);

            Marshal.StructureToPtr(value, valuePointer, false);
            Marshal.Copy(valuePointer, structureBytes, 0, ValueSize);
            return structureBytes;
        }

        private T GetT(byte[] bytes) 
        {
            IntPtr objectPointer = Marshal.AllocHGlobal(ValueSize);
            Marshal.Copy(bytes, 0, objectPointer, ValueSize);

            return (T)Marshal.PtrToStructure<T>(objectPointer);
        }

        public void Dispose() 
        {
            directory.CacheFileStream.Dispose();
        }
    }
}

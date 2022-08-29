using TeleBufet.NET.CacheManager.Interfaces;
using DatagramsNet;
using System.Reflection;
using TeleBufet.NET.CacheManager.Attributes;

namespace TeleBufet.NET.CacheManager
{

    internal class CacheHelper<T> : IDisposable
    {
        public T CacheValue { get; set; }

        protected ICacheDirectory directory;
        private static readonly Type[] attributeTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(n => n.GetTypes().Where(n => n.GetCustomAttributes(typeof(CacheTableAttribute), true).Length > 0)).ToArray();

        public Type CurrentType;

        public CacheHelper() 
        {
            var currentCacheDirectory = GetCurrentCacheDirectory();
            if (currentCacheDirectory is not null)
                directory = currentCacheDirectory;
            else
                throw new Exception("This type doesn't have any caching file");
        }

        public CacheHelper(T value) 
        {
            CacheValue = value;
            directory = GetCurrentCacheDirectory();
        }

        protected ICacheDirectory? GetCurrentCacheDirectory() 
        {
            if (CurrentType is null) 
            {
                for (int i = 0; i < attributeTypes.Length; i++)
                {
                    var currentAttribute = (CacheTableAttribute)attributeTypes[i].GetCustomAttribute(typeof(CacheTableAttribute));
                    if (currentAttribute.TableType == typeof(T))
                        CurrentType = attributeTypes[i];
                }
            }

            return (ICacheDirectory)Activator.CreateInstance(CurrentType);
        }

        protected virtual void SetBinarySeek() => directory.CacheFileStream.Seek(0, SeekOrigin.End);

        public virtual void Serialize() 
        {
            SetBinarySeek();
            using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            byte[] bytes = BinaryHelper.Write(CacheValue);
            binaryWriter.Write(bytes);

            directory.LastChanges = DateTime.UtcNow.TimeOfDay;
        }

        public virtual T[] Deserialize() 
        {
            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();

            directory.CacheFileStream.Seek(0, SeekOrigin.Begin);
            using var binaryReader = new BinaryReader(directory.CacheFileStream);
            int length = (int)directory.CacheFileStream.Length;
            Span<byte> spanBytes = binaryReader.ReadBytes(length).AsSpan();
            return BinaryHelper.Read<T[]>(spanBytes.ToArray());
        }

        public void RemoveBytes(int startIndex, int length) 
        {
            var shiftBytes = GetShiftBytes(startIndex, length).Span;

            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();

            directory.CacheFileStream.Seek(startIndex, SeekOrigin.Begin);
            using var binaryWriter = new BinaryWriter(directory.CacheFileStream);
            int fileLength = (int)directory.CacheFileStream.Length;
            binaryWriter.Write(shiftBytes);

            directory.CacheFileStream.SetLength(fileLength - length);
        }

        protected Memory<byte> GetShiftBytes(int startIndex, int length) 
        {
            if (!directory.CacheFileStream.CanRead)
                directory = GetCurrentCacheDirectory();

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

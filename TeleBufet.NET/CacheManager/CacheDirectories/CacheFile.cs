using TeleBufet.NET.CacheManager.Interfaces;

namespace TeleBufet.NET.CacheManager.CacheDirectories
{
    internal abstract class CacheFile : ICacheDirectory
    {
        public string SystemCacheDirectory => FileSystem.CacheDirectory;

        public abstract string FileName { get; }

        public Stream CacheFileStream { get; set; } 

        // I know that File has a FileOrCreate, but this makes it a little bit clear
        public CacheFile() 
        {
            var path = $"{SystemCacheDirectory}{FileName}";
            if (!(File.Exists(path)))
                CacheFileStream = File.Create(path);
            else
                CacheFileStream = File.Open(path, FileMode.Open);
        }
    }
}

using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Images")]
    public sealed class ImageTable : ICacheTable<TimeSpan>
    {
        [DataColumn("ImagesId")]
        public int Id { get; set; }

        [DataColumn("Url")]
        public string Source { get; set; }

        [DataColumn("TimeStamp")]
        public TimeSpan Key { get; set; }
    }
}

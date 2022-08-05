using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Images")]
    internal sealed class ImageTable : ITable
    {
        [DataColumn("ImagesId")]
        public int Id { get; set; }

        [DataColumn("Url")]
        public string Source { get; set; }
    }
}

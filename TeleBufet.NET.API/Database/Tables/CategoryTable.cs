using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Categories")]
    public class CategoryTable : ITable, ICache<TimeSpan> 
    {
		[DataColumn("CategoryId")]
        public int Id { get; set; }

        [DataColumn("Name")]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;

        [DataColumn("ImagesId")]
        public int ImageId { get; set; }

		[DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }
    }
}

using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [Table("Category")]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CategoryTable : ITable, ICache<TimeSpan> 
    {
		[DataColumn("CategoryId")]
        public int Id { get; set; }

        [DataColumn("Name")]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;

        [DataColumn("ImagesId")]
        public int ImageId { get; set; }

		[DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }
    }
}

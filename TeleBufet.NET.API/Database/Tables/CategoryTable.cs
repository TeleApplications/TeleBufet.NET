using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Categories")]
    public sealed class CategoryTable : ITable, ICache<TimeSpan> 
    {
		[DataColumn("CategoryId")]
        public int Id { get; set; }

        [DataColumn("Category")]
        public string Name { get; set; }

        [DataColumn("ImagesId")]
        public int ImageId { get; set; }

		[DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }
    }
}

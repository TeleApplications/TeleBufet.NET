using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Products")]
    public sealed class ProductTable : ICacheTable<TimeSpan> 
    {
		[DataColumn("ProductID")]
        public int Id { get; set; }

        [DataColumn("Name")]
        public string Name { get; set; }

		[DataColumn("CategoryID")]
        public int CategoryId { get; set; }

		[DataColumn("ImagesID")]
        public int ImageId { get; set; }

		[DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }
    }
}

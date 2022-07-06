using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{

    //[Table("Users")]
    [StructLayout(LayoutKind.Sequential)]
    public struct ProductTable : ITable, ICache<TimeSpan> 
    {
		[DataColumn("ProductId")]
        public int Id { get; set; }

		[DataColumn("Name")]
        public string Name { get; set; }

		[DataColumn("CategoryID")]
        public int CategoryId { get; set; }

		[DataColumn("Price")]
        public double Price { get; set; }

		[DataColumn("Amount")]
        public int Amount { get; set; }

		[DataColumn("ImagesID")]
        public int ImageId { get; set; }

		[DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }
    }
}

using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Products")]
    public class ProductTable : ITable, ICache<TimeSpan> 
    {
		[DataColumn("ProductID")]
        public int Id { get; set; }

        [DataColumn("Name")]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Name;

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

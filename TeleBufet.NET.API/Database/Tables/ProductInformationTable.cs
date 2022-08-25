using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;
using TeleBufet.NET.API.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("ProductsInformation")]
    public sealed class ProductInformationTable : ICacheTable<TimeSpan>
    {
        [DataColumn("Id")]
        public int Id { get; set; }

        [DataColumn("Price")]
        public double Price { get; set; }

        [DataColumn("Amount")]
        public int Amount { get; set; }

        [DataColumn("Timestamp")]
        public TimeSpan Key { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not ProductInformationTable newTable)
                return false;
            int newValidationNumber = (this.Amount + (int)this.Price);
            int originalValidationNumber = (newTable.Amount + (int)newTable.Price);

            return (newValidationNumber == originalValidationNumber);
        }
    }
}

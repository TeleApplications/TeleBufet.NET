using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Table("Orders")]
    public sealed class OrderTable : ITable
    {
        [DataColumn("OrderNumber")]
        public int Id { get; set; }

        [DataColumn("UserID")]
        public int UserId { get; set; }

        [DataColumn("ProductID")]
        public int ProductId { get; set; }

        [DataColumn("Amount")]
        public int Amount { get; set; }

        [DataColumn("ReservedTime")]
        public int ReservedTime { get; set; }
    }
}

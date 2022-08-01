using System.Runtime.InteropServices;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16, CharSet = CharSet.Unicode)]
    [Table("Users")]
    public sealed class UserTable : ITable
    {
		[DataColumn("UserId")]
        public int Id { get; set; }

		[DataColumn("Email")]
        public string Email { get; set; }

		[DataColumn("Karma")]
        public int Karma { get; set; }

		[DataColumn("Token")]
        public string Token { get; set; }
    }
}

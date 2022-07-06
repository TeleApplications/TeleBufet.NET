using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [Table("programming_language")]
	public sealed class ProgrammingLanguageTable : ITable
	{
		[DataColumn("Id_ProgrammingLanguage")]
		public int Id { get; set; } = 2;

		[DataColumn("IdType")]
		public int IdType { get; set; }

		[DataColumn("Name")]
		public string Name { get; set; }
	}
}

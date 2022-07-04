using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.Tables
{
    [Table("programmer")]
    public sealed class ProgrammerTable : ITable
    {
        [DataColumn("Id_Programmer")]
        public int Id { get; set; } = 5;

        [DataColumn("FirstName")]
        public string FirstName { get; set; }

        [DataColumn("LastName")]
        public string LastName { get; set; }

        [DataColumn("BirthDate")]
        public DateTime BirthDay { get; set; }

        [DataColumn("Id_ProgrammingLanguage")]
		[Key(typeof(ProgrammingLanguageTable), new string[] {"Name", "Realease_Date", "DevelopmentCompany"})] 
        public string LanguageName { get; set; }
    }
}

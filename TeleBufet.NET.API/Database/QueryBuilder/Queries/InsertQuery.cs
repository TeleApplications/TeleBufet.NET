using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder.Queries
{
    public sealed class InsertQuery<T> : QueryBuilder<T> where T : ITable, new() 
    {
        public object[] Data { get; set; }

        public InsertQuery(T table) : base(table) {}

        public override string CreateQuery()
        {
            string insertQuery = $"INSERT INTO {CurrentTableInformation.Name} VALUES ("; //TODO: Json impementation
            for (int i = 0; i < Data.Length; i++)
            {
                if (Data[i].GetType() == typeof(DateTime))
                    Data[i] = ((DateTime)Data[i]).ToString("yyyy-MM-dd");
                insertQuery = insertQuery + $"'{Data[i]}', ";
            }
            return $"{insertQuery[0..(insertQuery.Length - 2)]});";
        }
    }
}

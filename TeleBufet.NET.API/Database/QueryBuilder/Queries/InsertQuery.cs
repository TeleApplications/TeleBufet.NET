using System.Text;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder.Queries
{
    public sealed class InsertQuery<T> : QueryBuilder<T> where T : ITable, new() 
    {
        private static readonly char separator = ',';

        public object[] Data { get; set; }

        public InsertQuery(T table) : base(table) {}

        public override string CreateQuery()
        {
            var columns = new StringBuilder();
            var values = new StringBuilder();

            var dataColumns = GetTablePropertiesAttributes<DataColumnAttribute>().ToArray();
            for (int i = 0; i < Data.Length; i++)
            {
                columns.Append($"{dataColumns[i].Name}");

                if (Data[i].GetType() == typeof(DateTime))
                    values.Append(((DateTime)Data[i]).ToString("yyyy-MM-dd"));
                else
                    values.Append($"'{Data[i]}'");

                if ((i + 1) != Data.Length) 
                {
                    columns.Append(separator);
                    values.Append(separator);
                }
            }
            return $"INSERT INTO {CurrentTableInformation.Name} ({columns}) VALUES ({values})"; //TODO: Json impementation
        }
    }
}

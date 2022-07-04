using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder.Queries
{
    public sealed class UpdateQuery<T> : QueryBuilder<T> where T : ITable 
    {
        private T originalTable;

        public UpdateQuery(T table, T _orignalTable) : base(table) { originalTable = _orignalTable; }

        public override string CreateQuery()
        {
            int orinalTableId = originalTable.Id;
            var columnNames = GetTablePropertiesAttributes<DataColumnAttribute>().ToArray();
            var columnData = GetTableProperties(tableHandler).ToArray();
            var originalColumnData = GetTableProperties(originalTable).ToArray();

            string updateQuery = $"UPDATE {GetTableName()} SET";

            for (int i = 0; i < columnNames.Length; i++)
            {
                var column = columnData[i].GetValue(tableHandler);
                if (column != originalColumnData[i].GetValue(originalTable)) 
                {
                    if (column.GetType() == typeof(DateTime))
                        column = ((DateTime)column).ToString("yyyy-MM-dd");
                    updateQuery = updateQuery + $" {columnNames[i].Name}='{column}',";
                }
            }
            return $"{updateQuery[0..(updateQuery.Length - 1)]} WHERE {columnNames[0].Name} = {orinalTableId}";
        }
    }
}

using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder.Queries
{
    public sealed class DeleteQuery<T> : QueryBuilder<T> where T : ITable, new()
    {
        public DeleteQuery(T table) : base(table) { }

        public override string CreateQuery()
        {
            int tableId = tableHandler.Id;
            var tableNames = GetTablePropertiesAttributes<DataColumnAttribute>().ToArray();

            return $"DELETE FROM {CurrentTableInformation.Name} WHERE {tableNames[0].Name} = {tableId}";
        }  
    }
}

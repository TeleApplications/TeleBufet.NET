using System.Reflection;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder.Queries
{
    public sealed class SelectQuery<T> : QueryBuilder<T> where T : ITable, new() 
    {
        public DataColumnAttribute[] Columns { get; set; }

        public SelectQuery(T table) : base(table) { }

        public override string CreateQuery()
        {
            var columns = GetTablePropertiesAttributes<DataColumnAttribute>().ToArray();
            var keyColumns = GetTablePropertiesAttributes<KeyAttribute>().ToArray();
            Columns = columns;

            string joinString = String.Empty;
            string selectQuery = $"SELECT ";
			for (int i = 0; i < columns.Length; i++)
			{
                string columnName = columns[i].Name;
                if (keyColumns[i] is not null) 
                {
                    string columnKeyJoinName = keyColumns[i].ConnectionTableType.Name[0..4] + columnName[0];
                    columnName = String.Empty;
                    for (int j = 0; j < keyColumns[i].ColumnNames.Length; j++)
                    {
                        columnName += $",{columnKeyJoinName}.{keyColumns[i].ColumnNames[j]}";
                    }
                    columnName = columnName[1..columnName.Length];
                    joinString = joinString + $" INNER JOIN {(keyColumns[i].ConnectionTableType.GetCustomAttribute
                        (typeof(TableAttribute)) as TableAttribute).TableName} {columnKeyJoinName} ON {columnKeyJoinName}.{columns[i].Name} = {CurrentTableInformation.Name}.{columns[i].Name}";
                }
                selectQuery = selectQuery + $"{columnName}, ";
			}
            selectQuery = $"{selectQuery[0..(selectQuery.Length - 2)]}  FROM {CurrentTableInformation.Name} {joinString} ORDER BY {columns[0].Name}";
            return selectQuery;
        }
    }
}

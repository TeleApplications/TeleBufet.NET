using System.Collections.Immutable;
using System.Reflection;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder
{
    public readonly struct TableInfomation 
    {
        public ITable Table { get; }
        public string Name { get; }
        public ReadOnlyMemory<PropertyInfo> Properties { get; }

        public TableInfomation(ITable table, string name, ReadOnlyMemory<PropertyInfo> properties) 
        {
            Table = table;
            Name = name;
            Properties = properties;
        }
    }

    public abstract class QueryBuilder<T> where T : ITable, new()
    {
        public TableInfomation CurrentTableInformation { get; }

        protected T tableHandler;
        private static List<TableInfomation> tableInformations = new();


        public QueryBuilder(T table) 
        {
            tableHandler = table;
            if (!TryGetTableInformation(out TableInfomation newTable)) 
            {
                newTable = new TableInfomation(table, GetTableName(), GetTableProperties(table));
                tableInformations.Add(newTable);
            }
            CurrentTableInformation = newTable;
        }

        public abstract string CreateQuery();

        private string GetTableName() 
        {
            return ((tableHandler
                .GetType()
                .GetCustomAttribute(typeof(TableAttribute))) as TableAttribute).TableName;
        }

        protected ReadOnlyMemory<PropertyInfo> GetTableProperties(T table) 
        {
            return table
                .GetType()
                .GetProperties()
                .Where(n => n
                .GetCustomAttributes(typeof(DataColumnAttribute), true).Length > 0).ToArray();
        }

        protected IEnumerable<TAttribute> GetTablePropertiesAttributes<TAttribute>() where TAttribute : Attribute
        {
			for (int i = 0; i < CurrentTableInformation.Properties.Length; i++)
			{
                yield return (TAttribute)(CurrentTableInformation.Properties.Span[i].GetCustomAttribute(typeof(TAttribute)));
			}
        }

        private bool TryGetTableInformation(out TableInfomation tableInformation) 
        {
            var tableHolder = new T();
            tableInformation = default;

            for (int i = 0; i < tableInformations.Count; i++)
            {
                var currentTable = tableInformations[i].Table;
                if (currentTable.Equals(tableHolder))
                    tableInformation = tableInformations[i];
            }
            return tableInformation.Equals(default);
        }
    }
}

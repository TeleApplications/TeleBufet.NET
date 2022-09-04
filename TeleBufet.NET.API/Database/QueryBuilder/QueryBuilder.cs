using System.Numerics;
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

        //This name is temporary
        private bool TryGetTableInformationVectorized(out TableInfomation tableInfomation) 
        {
            var tablesId = GetTablesId(tableInformations.ToArray()).ToArray();

            int handlerId = tableHandler.Id;
            var vectorHolder = new Vector<int>(handlerId);

            int vectorDifference = tableInformations.Count - Vector<int>.Count;
            for (int i = 0; i < vectorDifference; i+= Vector<int>.Count)
            {
                var currentTable = new Vector<int>(tablesId, i);
                Vector<int> equalValue = Vector.Equals<int>(vectorHolder, currentTable);
            }

            tableInfomation = new();
            return false;
        }

        private static IEnumerable<int> GetTablesId(TableInfomation[] informations) 
        {
            for (int i = 0; i < informations.Length; i++)
            {
                yield return informations[i].Table.Id;
            }
        }
    }
}

using System.Collections.Immutable;
using System.Reflection;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder
{
    internal sealed class TableInfomation 
    {
        public ITable Table { get; set; }

        public string Name { get; set; }

        public PropertyInfo[] Properties { get; set; }

        public Attribute[] Attributes { get; set; }
    }

    public abstract class QueryBuilder<T> where T : ITable
    {
        protected T tableHandler;

        private static List<TableInfomation> tableInformations = new();

        private TableInfomation currentTable;

        public QueryBuilder(T table) 
        {
            tableHandler = table;
            currentTable = TryGetTableInformation(table);
        }

        public abstract string CreateQuery();

        protected virtual string GetTableName() 
        {
            if(currentTable.Name is null)
                currentTable.Name = ((tableHandler.GetType().GetCustomAttribute(typeof(TableAttribute))) as TableAttribute).TableName;
            return currentTable.Name;
        }

        public PropertyInfo[] GetTableProperties(T table) 
        {
            if(currentTable.Properties is null)
                currentTable.Properties = table.GetType().GetProperties().Where(n => n.GetCustomAttributes(typeof(DataColumnAttribute), true).Length > 0).ToArray();
            return currentTable.Properties;
        }

        protected IEnumerable<TAttribute> GetTablePropertiesAttributes<TAttribute>() where TAttribute : Attribute
        {
            var proerties = GetTableProperties(tableHandler);
			for (int i = 0; i < proerties.Length; i++)
			{
                yield return (TAttribute)(proerties[i].GetCustomAttribute(typeof(TAttribute)));
			}
        }

        private TableInfomation TryGetTableInformation(T table) 
        {
            int index = tableInformations.Count - 1;
            var cacheTable = tableInformations.FirstOrDefault(n => n.Table.Equals(table))!;
            if (cacheTable is null) 
            {
                tableInformations.Add(new TableInfomation() { Table = table });
                index++;
            }
            return tableInformations[index];
        }
    }
}

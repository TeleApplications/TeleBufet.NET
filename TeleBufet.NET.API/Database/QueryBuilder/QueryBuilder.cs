using System.Reflection;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database.QueryBuilder
{
    public abstract class QueryBuilder<T> where T : ITable
    {
        protected T tableHandler;

        public QueryBuilder(T table) => tableHandler = table;

        public abstract string CreateQuery();

        protected virtual string GetTableName() 
        {
            var tableAttribute = (tableHandler.GetType().GetCustomAttribute(typeof(TableAttribute))) as TableAttribute;
            return tableAttribute.TableName;
        }

        public PropertyInfo[] GetTableProperties(T table) 
        {
            return table.GetType().GetProperties().Where(n => n.GetCustomAttributes(typeof(DataColumnAttribute), true).Length > 0).ToArray();
        }

        protected IEnumerable<TAttribute> GetTablePropertiesAttributes<TAttribute>() where TAttribute : Attribute
        {
            var proerties = GetTableProperties(tableHandler);
			for (int i = 0; i < proerties.Length; i++)
			{
                yield return (TAttribute)(proerties[i].GetCustomAttribute(typeof(TAttribute)));
			}
        }
    }
}

namespace TeleBufet.NET.API.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DataColumnAttribute : Attribute
    {
        public string Name { get; }

        public DataColumnAttribute(string name) 
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute 
    {
        public Type ConnectionTableType { get; }
        public string[] ColumnNames { get; }

        public KeyAttribute(Type tableType, string[] columnNames) 
        {
            ConnectionTableType = tableType;
            ColumnNames = columnNames;
        }
    }
}

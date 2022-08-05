namespace TeleBufet.NET.API.Database.Attributes
{
	[Flags]
    public enum KeyState 
    {
        NONE = 0,
        FOREIGN_KEY = 1,
        UNIQUE_KEY = 3
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class DataColumnAttribute : Attribute
    {
        public string Name { get; set; }

        public DataColumnAttribute(string name) 
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute 
    {
        public Type ConnectionTableType { get; set; }
        public string[] ColumnNames { get; set; }

        public KeyAttribute(Type tableType, string[] columnNames) 
        {
            ConnectionTableType = tableType;
            ColumnNames = columnNames;
        }
    }
}

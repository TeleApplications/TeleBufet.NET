namespace TeleBufet.NET.API.Database.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; }

        public TableAttribute(string tableName) 
        {
            TableName = tableName;
        }
    }
}

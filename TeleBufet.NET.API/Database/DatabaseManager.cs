using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;
using TeleBufet.NET.API.Database.QueryBuilder.Queries;
using TeleBufet.NET.API.Database.Attributes;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database
{
    public struct UnknownData 
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }

    public sealed class DatabaseManager<T> : IDisposable where T : ITable, new()
    {

        private MySqlConnection mySqlConnection;

        public DataColumnAttribute[] ColumnNames { get; private set; }

        private MySqlConnectionStringBuilder connectionString;

        public List<UnknownData[]> UnknownData { get; private set; } 

        public DatabaseManager() 
        {
            connectionString = new MySqlConnectionStringBuilder() //TODO: This is termorary and of course not secure enough
            {
                Server = "localhost",
                Port = 3306,
                UserID = "root",
                Password = "",
                Database ="telebufettestdatabase",
                SslMode = MySqlSslMode.Preferred
            };
            mySqlConnection = new(connectionString.ToString());
        }

        public async Task<T[]> GetTable() 
        {
            mySqlConnection.Open();

            var dataTable = new DataTable();
            var tableHandler = new T();
            var selectQuery = new SelectQuery<T>(tableHandler);
            string query = selectQuery.CreateQuery();

            ColumnNames = selectQuery.Columns;
            using (var databaseAdapter = new MySqlDataAdapter(query, mySqlConnection)) 
            {
                databaseAdapter.Fill(dataTable);
                databaseAdapter.Dispose();
            }
            var properties = selectQuery.GetTableProperties(tableHandler);
            var tableData = GetTableData(dataTable, properties).ToArray();
            mySqlConnection.Close();
            return tableData; 
        }

        public async Task SetTable(T table) 
        {
            mySqlConnection.Open();

            var insertQuery = new InsertQuery<T>(table);
            object[] tableData = SetTableData(table, insertQuery.GetTableProperties(table)).ToArray();
            insertQuery.Data = tableData;
            var query = insertQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(query, mySqlConnection)) 
            {
                await databaseCommand.ExecuteNonQueryAsync();
                databaseCommand.Dispose();
            }
                mySqlConnection.Close();
        }

        public async Task UpdateTable(T table, T originalTable) 
        {
            mySqlConnection.Open();

            var updateQuery = new UpdateQuery<T>(table, originalTable);
            string queryString = updateQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(queryString, mySqlConnection)) 
            {
                databaseCommand.ExecuteScalar();
                databaseCommand.Dispose();
            }
                mySqlConnection.Close();
        }

        public async Task DeleteTable(T table) 
        {
            mySqlConnection.Open();

            var deleteQuery = new DeleteQuery<T>(table);
            string queryString = deleteQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(queryString, mySqlConnection)) 
            {
                databaseCommand.ExecuteScalar();
                databaseCommand.Dispose();
            }
            mySqlConnection.Close();
        }

        private IEnumerable<T> GetTableData(DataTable table, PropertyInfo[] properties) 
        {
            int offsetSize = (table.Columns.Count + 1) - properties.Length;
            var unknownData = new List<UnknownData[]>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var newTable = (object)new T();
                unknownData.Add(GetUnknownData(table.Rows, table.Columns, i, table.Columns.Count - offsetSize).ToArray());
                for (int j = 0; j < properties.Length; j++)
                {
                    properties[j].SetValue(newTable, table.Rows[i][table.Columns[j]]);
                }
                yield return (T)newTable;
            }
            UnknownData = unknownData;
        }

        private IEnumerable<UnknownData> GetUnknownData(DataRowCollection rows, DataColumnCollection columns, int index, int startIndex)
        {
            for (int i = startIndex; i < columns.Count; i++)
            {
                yield return new UnknownData() { Name = "s", Value = rows[index][columns[i]] };
            }
        }

        private IEnumerable<object> SetTableData(T table, PropertyInfo[] properties) 
        {
            for (int i = 0; i < properties.Length; i++)
            {
                yield return properties[i].GetValue(table);
            }
        }

        public async void Dispose() //TODO: Create better implementation in dispose
        {
            if (mySqlConnection is not null)
                await mySqlConnection.CloseAsync();
        }
    }
}

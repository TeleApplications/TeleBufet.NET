using MySql.Data.MySqlClient;
using System.Data;
using System.Reflection;
using TeleBufet.NET.API.Database.QueryBuilder.Queries;
using TeleBufet.NET.API.Database.Interfaces;

namespace TeleBufet.NET.API.Database
{
    public sealed class DatabaseManager<T> : IDisposable where T : ITable, new()
    {
        private MySqlConnectionStringBuilder connectionString;
        private static MySqlConnection mySqlConnection;

        public DatabaseManager() 
        {
            if (connectionString is null) 
            {
                //TODO: This is really temporary, but in the future it will database informations
                //from probably json
                connectionString = new MySqlConnectionStringBuilder()
                {
                    Server = "localhost",
                    Port = 3306,
                    UserID = "root",
                    Password = "",
                    Database ="telebufettestdatabase",
                    SslMode = MySqlSslMode.Preferred
                };
            }
            mySqlConnection = new(connectionString.ToString());
        }

        public async ValueTask<ReadOnlyMemory<T>> GetTable() 
        {
            await mySqlConnection.OpenAsync();

            var dataTable = new DataTable();
            var tableHandler = new T();
            var selectQuery = new SelectQuery<T>(tableHandler);
            string query = selectQuery.CreateQuery();

            using (var databaseAdapter = new MySqlDataAdapter(query, mySqlConnection)) 
            {
                await databaseAdapter.FillAsync(dataTable);
                databaseAdapter.Dispose();
            }
            var tables = CreateTables(dataTable, selectQuery.CurrentTableInformation.Properties);
            await mySqlConnection.CloseAsync();
            return tables; 
        }

        public async Task SetTable(T table) 
        {
            await mySqlConnection.OpenAsync();

            var insertQuery = new InsertQuery<T>(table);
            object[] tableData = GetTableData(table, insertQuery.CurrentTableInformation.Properties).ToArray();
            insertQuery.Data = tableData;
            var query = insertQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(query, mySqlConnection)) 
            {
                await databaseCommand.ExecuteNonQueryAsync();
                databaseCommand.Dispose();
            }
            await mySqlConnection.CloseAsync();
        }

        public async Task UpdateTable(T table, T originalTable) 
        {
            await mySqlConnection.OpenAsync();

            var updateQuery = new UpdateQuery<T>(table, originalTable);
            string queryString = updateQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(queryString, mySqlConnection)) 
            {
                await databaseCommand.ExecuteScalarAsync();
                databaseCommand.Dispose();
            }
            await mySqlConnection.CloseAsync();
        }

        public async Task DeleteTable(T table) 
        {
            await mySqlConnection.OpenAsync();

            var deleteQuery = new DeleteQuery<T>(table);
            string queryString = deleteQuery.CreateQuery();
            using (var databaseCommand = new MySqlCommand(queryString, mySqlConnection)) 
            {
                await databaseCommand.ExecuteScalarAsync();
                databaseCommand.Dispose();
            }
            await mySqlConnection.CloseAsync();
        }

        private ReadOnlyMemory<T> CreateTables(DataTable table, ReadOnlyMemory<PropertyInfo> properties) 
        {
            Memory<T> tables = new T[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var currentTable = new T();
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    var tableData = table.Rows[i][table.Columns[j]];
                    properties.Span[j].SetValue(currentTable, tableData);
                }
                tables.Span[i] = currentTable;
            }
            return tables;
        }

        private static IEnumerable<object> GetTableData(T table, ReadOnlyMemory<PropertyInfo> properties) 
        {
            for (int i = 0; i < properties.Length; i++)
            {
                yield return properties.Span[i].GetValue(table);
            }
        }

        public async void Dispose() //TODO: Create better implementation in dispose
        {
            if (mySqlConnection is not null)
                await mySqlConnection.CloseAsync();
        }
    }
}

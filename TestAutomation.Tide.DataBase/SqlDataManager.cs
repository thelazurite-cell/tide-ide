using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static TestAutomation.Tide.DataBase.InternalCommandManager;

namespace TestAutomation.Tide.DataBase
{
    public class SqlDataManager : IDisposable
    {
        public String ConnectionName { get; set; }
        private readonly SqlConnection sqlClient;

        public SqlDataManager(string connectionName, string connectionString)
        {
            this.ConnectionName = connectionName;
            this.sqlClient = new SqlConnection(connectionString);
        }

        public async Task OpenConnection()
        {
            try
            {
                await this.sqlClient.OpenAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public async Task<IEnumerable<TablePrimaryKey>> GetPrimaryKeys(string databaseName, string tableName)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            var list = new List<TablePrimaryKey>();
            var command = new SqlCommand(GetInternalCommand(nameof(this.GetPrimaryKeys)), this.sqlClient);
            this.sqlClient.ChangeDatabase(databaseName);
            command.Parameters.AddWithValue("@tableName", tableName);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    bool.TryParse(reader["is_unique"].ToString(), out var isUnique);
                    bool.TryParse(reader["is_primary_key"].ToString(), out var isPrimaryKey);
                    bool.TryParse(reader["is_unique_constraint"].ToString(), out var isUniqueConstraint);
                    var indexCols = reader["IndexColumnsNames"].ToString();
                    string[] indexes = {string.Empty};
                    if (!string.IsNullOrWhiteSpace(indexCols))
                    {
                        indexes = indexCols.Split(',').Select(itm => itm.Trim())
                            .Where(itm => !string.IsNullOrWhiteSpace(itm))
                            .ToArray();
                    }

                    var includedCols = reader["IndexColumnsNames"].ToString();
                    string[] includedIndexes = {string.Empty};
                    if (!string.IsNullOrWhiteSpace(includedCols))
                    {
                        includedIndexes = includedCols.Split(',').Select(itm => itm.Trim())
                            .Where(itm => !string.IsNullOrWhiteSpace(itm))
                            .ToArray();
                    }

                    var name = reader["IndexName"].ToString();

                    var pks = new TablePrimaryKey
                    {
                        DatabaseName = databaseName,
                        TableName = tableName,
                        Name = name,
                        IndexColumnsNames = indexes,
                        IncludedColumnsNames = includedIndexes,
                        IsUnique = isUnique,
                        IsPrimaryKey = isPrimaryKey,
                        IsUniqueConstraint = isUniqueConstraint,
                    };
                    list.Add(pks);
                }
            }

            return list;
        }

        public async Task<bool> GetAutoIncrementCheck(string databaseName, string columnName, string tableName)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            this.sqlClient.ChangeDatabase(databaseName);

            var incrCmd = new SqlCommand(GetInternalCommand(nameof(this.GetAutoIncrementCheck))) {Connection = this.sqlClient};
            incrCmd.Parameters.AddWithValue("@ColumnName", columnName);
            incrCmd.Parameters.AddWithValue("@TableName", tableName);

            using (var incrReader = await incrCmd.ExecuteReaderAsync())
            {
                if (!incrReader.HasRows) return false;
                while (await incrReader.ReadAsync())
                {
                    var incVal = incrReader["increment_value"].ToString();
                    int.TryParse(incVal, out var incrementValue);
                    return incrementValue > 0;
                }
            }

            return false;
        }

        public async Task<IEnumerable<TableConstraint>> GetConstraints(string databaseName, string tableName)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            var list = new List<TableConstraint>();
            var command = new SqlCommand(GetInternalCommand(nameof(this.GetConstraints)), this.sqlClient);
            this.sqlClient.ChangeDatabase(databaseName);
            command.Parameters.AddWithValue("@tableName", tableName);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var cnst = new TableConstraint()
                    {
                        SchemaName = reader["schema_name"].ToString(),
                        TableView = reader["table_view"].ToString(),
                        ObjectType = reader["object_type"].ToString(),
                        ConstraintType = reader["constraint_type"].ToString(),
                        Name = reader["constraint_name"].ToString(),
                        Details = reader["details"].ToString()
                    };
                    list.Add(cnst);
                }
            }

            return list;
        }

        public async Task<IEnumerable<TableForeignKey>> GetForeignKeys(string databaseName, string tableName)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            var list = new List<TableForeignKey>();
            var command = new SqlCommand(GetInternalCommand(nameof(this.GetForeignKeys)), this.sqlClient);
            this.sqlClient.ChangeDatabase(databaseName);
            command.Parameters.AddWithValue("@tableName", tableName);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var fks = new TableForeignKey
                    {
                        DatabaseName = databaseName,
                        PrimaryColumn = reader["PK_Column"].ToString(),
                        PrimaryTable = reader["PK_Table"].ToString(),
                        ForeignColumn = reader["FK_Column"].ToString(),
                        ForeignTable = reader["K_Table"].ToString(),
                        Name = reader["Constraint_Name"].ToString()
                    };
                    list.Add(fks);
                }
            }

            return list;
        }

        public async Task<DtoCollection> GetTableData(DataTable table, ObservableCollection<DbView> columns,
            TablePagination tablePagination)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var colNames = columns.ToList().Select(itm => (itm as TableColumn)?.QueryFriendlyName).ToList();
            var primaryColNames = columns.OfType<TableColumn>().Where(itm => itm.IsPrimaryKey)
                .Select(itm => itm?.QueryFriendlyName).ToList();

            var @join = string.Join(",", colNames);
            string str;

            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            this.sqlClient.ChangeDatabase(table.DatabaseName);
            var paginationCol = "__TablePaginationOrderNumber";

            if (tablePagination.IsEnabled)
                str = await this.GeneratePaginatedCommand(table, tablePagination, @join, paginationCol,
                    primaryColNames);
            else
                str = $"SELECT {join} FROM [{table.Name}]";

            Console.WriteLine(str);

            var command = new SqlCommand(str, this.sqlClient);
            return await this.ExecuteGetDataCommand(table, columns, tablePagination, command, str);
        }

        private async Task<DtoCollection> ExecuteGetDataCommand(DataTable table, ObservableCollection<DbView> columns,
            TablePagination tablePagination,
            SqlCommand command, string str)
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                var dataView = new DataView
                {
                    Headers = columns,
                    Table = table
                };
                while (await reader.ReadAsync())
                {
                    var temp = columns.Select(itm => itm.Name).Select(colName =>
                        new KeyValuePair<string, object>(colName,
                            reader[colName] is DBNull ? null : reader[colName])).ToList();
                    dataView.Results.Add(temp);
                }

                var results = DtoCreator.CreateResultSet(dataView);
                results.SqlCommand = str;
                results.ConnectionString = this.sqlClient.ConnectionString;
                results.ConnectionName = this.ConnectionName;
                results.Pagination = tablePagination;
                results.Columns = columns;

                return results;
            }
        }

        private async Task<string> GeneratePaginatedCommand(DataTable table, TablePagination tablePagination,
            string @join,
            string paginationCol, List<string> primaryColNames)
        {
            var str = $@"
SELECT {@join}  FROM (SELECT {@join},{paginationCol}
FROM (SELECT";
            str += @join + ", ROW_NUMBER() OVER(ORDER BY ";
            if (primaryColNames.Any())
                str += string.Join(",", primaryColNames);
            else
                str += @join;
            str += $") {paginationCol}\n";

            str +=
                $"FROM [{table.Name}]) t0\n WHERE {paginationCol} > {tablePagination.StartingIndex} AND {paginationCol} <= {tablePagination.EndingIndex}) t1";

            var countStr = $"SELECT COUNT(*) __COUNT FROM [{table.Name}]";
            var countCommand = new SqlCommand(countStr, this.sqlClient);
            using (var reader = await countCommand.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int.TryParse(reader["__COUNT"].ToString(), out var count);
                    tablePagination.RowsCount = count;
                    break;
                }
            }

            return str;
        }

        public async Task<IEnumerable<TableColumn>> GetTableColumns(DataTable table)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            var list = new List<TableColumn>();
            var command = new SqlCommand(GetCmdText(table.TableType), this.sqlClient);

            this.sqlClient.ChangeDatabase(table.DatabaseName);
            command.Parameters.AddWithValue("@tableName", table.Name);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int.TryParse(reader["column_id"].ToString(), out var columnId);
                    Enum.TryParse(reader["data_type"].ToString(), out DataType dataType);
                    int.TryParse(reader["max_length"].ToString(), out var maxLength);
                    int.TryParse(reader["precision"].ToString(), out var precision);
                    int.TryParse(reader["object_id"].ToString(), out var objectId);
                    bool.TryParse(reader["is_nullable"].ToString(), out var isNullable);
                    bool.TryParse(reader["is_ansi_padded"].ToString(), out var isAnsiPadded);
                    bool.TryParse(reader["is_rowguidcol"].ToString(), out var isRowGuidCol);
                    bool.TryParse(reader["is_identity"].ToString(), out var isIdentity);

                    var col = new TableColumn
                    {
                        ColumnId = columnId,
                        Database = table.DatabaseName,
                        SchemaName = reader["schema_name"].ToString(),
                        TableName = reader["table_name"].ToString(),
                        Name = reader["column_name"].ToString(),
                        DataType = dataType,
                        MaxLength = maxLength,
                        Precision = precision,
                        ObjectId = objectId,
                        CollationName = reader["collation_name"].ToString(),
                        DefaultValue = reader["default_definition"].ToString(),
                        IsNullable = isNullable,
                        IsAnsiPadding = isAnsiPadded,
                        IsRowGuidCol = isRowGuidCol,
                        IsIdentity = isIdentity
                    };

                    list.Add(col);
                }
            }

            return list;
        }

        private static string GetCmdText(DataTableType tableType)
        {
            var tableCommand = GetInternalCommand("GetTableColumns");
            var viewCommand = GetInternalCommand("GetViewColumns");
            return tableType == DataTableType.Table ? tableCommand : viewCommand;
        }

        public async Task<ObservableCollection<DataTable>> GetDatabaseTables(string databaseName)
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            this.sqlClient.ChangeDatabase(databaseName);
            var list = new ObservableCollection<DataTable>();
            var command = new SqlCommand(GetInternalCommand(nameof(this.GetDatabaseTables)), this.sqlClient);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var tbl = new DataTable
                    {
                        DatabaseName = databaseName,
                        Name = reader["TABLE_NAME"].ToString(),
                        Schema = reader["TABLE_SCHEMA"].ToString(),
                        TableType = reader["TABLE_TYPE"].ToString() == "BASE TABLE"
                            ? DataTableType.Table
                            : DataTableType.View
                    };
                    list.Add(tbl);
                }
            }

            return list;
        }

        public async Task<IEnumerable<Database>> GetDatabases()
        {
            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            var list = new List<Database>();
            var command = new SqlCommand(GetInternalCommand(nameof(this.GetDatabases)), this.sqlClient);
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int.TryParse(reader["database_id"].ToString(), out var dbId);
                    DateTime.TryParse(reader["create_date"].ToString(), out var creationDate);
                    var db = new Database
                    {
                        Name = reader["name"].ToString(),
                        Id = dbId,
                        CreationDate = creationDate,
                        CollationName = reader["collation_name"].ToString()
                    };
                    list.Add(db);
                }
            }

            return list;
        }

        public async Task<int> PerformNonQuery(SqlCommand command, string databaseName)
        {
            command.Connection = this.sqlClient;

            if (this.sqlClient.State == ConnectionState.Closed)
                await this.OpenConnection();
            this.sqlClient.ChangeDatabase(databaseName);

            var result = await command.ExecuteNonQueryAsync();
            return result;
        }

        public void Dispose()
        {
            if (this.sqlClient == null)
            {
                return;
            }

            if (this.sqlClient?.State != ConnectionState.Closed)
            {
                this.sqlClient?.Close();
            }

            this.sqlClient?.Dispose();
        }

        ~SqlDataManager()
        {
            this.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
//using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TestAutomation.Tide.DataBase;
using TestAutomation.Tide.DataBase.Properties;

namespace TestAutomation.Tide.DataBase
{
    public abstract class DataTransferObject : INotifyPropertyChanged
    {
        private bool isIdInsertAllowed;
        private bool isDeleting;
        private bool isInserting;
        public bool Clean { get; private set; } = true;
        public event PropertyChangedEventHandler PropertyChanged;
        public EventHandler UpdateFailed;
        public EventHandler InsertFailed;
        public EventHandler DeleteFailed;

        public bool IsIdInsertAllowed
        {
            get => this.isIdInsertAllowed;
            set
            {
                this.isIdInsertAllowed = value;
                this.OnPropertyChanged(nameof(this.IsIdInsertAllowed));
            }
        }

        public bool IsDeleting
        {
            get => this.isDeleting;
            set
            {
                this.isDeleting = value;
                this.OnPropertyChanged(nameof(this.IsDeleting));
            }
        }

        public bool IsInserting
        {
            get => this.isInserting;
            set
            {
                this.isInserting = value;
                this.OnPropertyChanged(nameof(this.IsInserting));
            }
        }

        public async Task Insert(DtoCollection context)
        {
            if (this.IsDeleting || !this.IsInserting) return;
            try
            {
                var properties = this.GetType().GetProperties();
                var insert =
                    $"INSERT INTO [{context.Table.DatabaseName}].[{context.Table.Schema}].[{context.Table.Name}] (";
                var values = "VALUES (";
                var fields = properties.Where(itm => itm.GetCustomAttribute<FieldNameAttribute>() != null).ToList();
                foreach (var field in fields)
                {
                    var columnDefinition = context.Columns.FirstOrDefault(itm => itm.Name == field.Name);
                    if (columnDefinition is TableColumn tableColumn)
                    {
                        if (tableColumn.IsGenerated) continue;

                        var attribute = field.GetCustomAttribute<FieldNameAttribute>();
                        insert += $"[{attribute.FieldName}]";
                        var value = field.GetValue(this);

                        if (value != null)
                            values += $"@Insert{field.Name}";
                        else if (tableColumn.HasDefaultValue)
                            values += tableColumn.DefaultValue;
                        else if (attribute.IsNullable)
                            values += "null";
                        else
                            throw new InvalidOperationException(
                                $"{attribute.FieldName} is not nullable, but we attempted to pass null to it.");

                        if (fields.IndexOf(field) == fields.Count - 1) continue;
                        insert += ", ";
                        values += ", ";
                    }
                }

                insert += ")";
                values += ")";
                var generatedString = $"{insert}\n{values}";

                var sql = new SqlCommand(generatedString);

                foreach (var field in fields)
                {
                    var value = field.GetValue(this);
                    if (value == null) continue;

                    var attribute = field.GetCustomAttribute<FieldNameAttribute>();

                    var sqlDbType = DataTypeHelper.TypeEnumConversion[attribute.DataType];

                    sql.Parameters.Add(new SqlParameter($"@Insert{field.Name}", sqlDbType)
                    {
                        Value = value
                    });
                }

                Console.WriteLine(sql.CommandAsSql());
                using (var conn = new SqlDataManager(context.ConnectionName, context.ConnectionString))
                {
                    var res = await conn.PerformNonQuery(sql, context.Table.DatabaseName);
                    Console.WriteLine($"{res} row(s) affected.");
                }
            }
            catch (Exception e)
            {
                var de = new DtoException() {DataTransferObject = this, Exception = e};
                this.InsertFailed?.Invoke(de, EventArgs.Empty);
            }
        }

        public async Task Update(List<SqlDataUpdate> updates, DtoCollection context)
        {
            if (this.IsDeleting || this.IsInserting) return;

            IEnumerable<TablePrimaryKey> primaryKeys;
            using (var conn = new SqlDataManager(context.ConnectionName, context.ConnectionString))
            {
                primaryKeys = await conn.GetPrimaryKeys(context.Table.DatabaseName, context.Table.Name);
            }

            var properties = this.GetType().GetProperties();
            var generatedString = this.CreateUpdateWhereClause(primaryKeys, properties,
                GenerateUpdateSetters(updates, context));
            var sql = new SqlCommand(generatedString);
            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<FieldNameAttribute>();
                if (attribute == null) continue;
                var originalProperty = this.GetType().GetProperty($"Original{property.Name}");
                var originalPropValue = originalProperty.GetValue(this);
                var update = updates.FirstOrDefault(itm => itm.Property == property.Name);

                var sqlDbType = DataTypeHelper.TypeEnumConversion[attribute.DataType];

                var whereClause = $"@Where{property.Name}";
                if (generatedString.Contains(whereClause) && originalPropValue != null)
                {
                    sql.Parameters.Add(
                        new SqlParameter(whereClause, sqlDbType)
                        {
                            Value = originalPropValue
                        });
                }

                if (update?.ColumnValue == null) continue;
                var clause = $"@{property.Name}";
                if (generatedString.Contains(clause))
                    sql.Parameters.Add(
                        new SqlParameter(clause, sqlDbType)
                        {
                            Value = update.ColumnValue
                        });
            }

            Console.WriteLine(sql.CommandAsSql());
            try
            {
                using (var conn = new SqlDataManager(context.ConnectionName, context.ConnectionString))
                {
                    var res = await conn.PerformNonQuery(sql, context.Table.DatabaseName);
                    Console.WriteLine($"{res} row(s) affected.");
                }
            }
            catch (Exception e)
            {
                this.UpdateFailed?.Invoke(new DtoException {DataTransferObject = this, Exception = e},
                    EventArgs.Empty);
            }
        }

        private static string GenerateUpdateSetters(List<SqlDataUpdate> updates, DtoCollection context)
        {
            var sql = $"UPDATE [{context.Table.DatabaseName}].[{context.Table.Schema}].[{context.Table.Name}]\r\nSET ";
            foreach (var update in updates)
            {
                var prop = context.DtoType.GetProperty(update.Property);
                var attribute = prop.GetCustomAttribute<FieldNameAttribute>();

                if (update.ColumnValue != null)
                    sql += $"[{attribute.FieldName}] = @{update.Property}";
                else
                    sql += $"[{attribute.FieldName}] = null";

                if (updates.IndexOf(update) != updates.Count - 1)
                    sql += ", ";
            }

            return sql;
        }

        private string CreateUpdateWhereClause(IEnumerable<TablePrimaryKey> primaryKeys, PropertyInfo[] properties,
            string sql)
        {
            // Identify the dto by any primary keys; otherwise we'll need to make sure the data is an exact match by checking every field available.
            sql += "\r\nWHERE ";

            if (primaryKeys.Any())
            {
                IEnumerable<PropertyInfo> primaryToProperty = properties.Where(property =>
                {
                    var attribute = property.GetCustomAttribute<FieldNameAttribute>();
                    if (attribute == null) return false;
                    return primaryKeys.FirstOrDefault(itm =>
                               itm.IndexColumnsNames.Any(key => key == attribute.FieldName)) != null;
                });
                foreach (var property in primaryToProperty)
                {
                    var originalProperty = this.GetType().GetProperty($"Original{property.Name}");
                    var originalPropValue = originalProperty.GetValue(this);
                    var lastValue = primaryToProperty.ToList().IndexOf(property) ==
                                    primaryToProperty.ToList().Count() - 1;
                    var attribute = property.GetCustomAttribute<FieldNameAttribute>();
                    if (attribute == null) continue;
                    if (originalPropValue == null)
                    {
                        if (!attribute.IsNullable)
                            throw new InvalidOperationException($"[{attribute.FieldName}] shouldn't be null!");
                        sql += $"[{attribute.FieldName}] IS NULL";
                    }
                    else
                    {
                        sql += $"[{attribute.FieldName}] = @Where{property.Name}";
                    }

                    if (!lastValue)
                        sql += $" AND ";
                }
            }
            else
            {
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<FieldNameAttribute>();
                    if (attribute == null) continue;
                    sql += $"[{attribute.FieldName}] = @Where{property.Name}";
                    if (properties.ToList().IndexOf(property) != properties.Length - 1)
                        sql += $" AND ";
                }
            }

            return sql;
        }

        public async Task Delete(SqlDataDelete delete, DtoCollection context)
        {
            try
            {
            }
            catch (Exception e)
            {
                this.DeleteFailed?.Invoke(new DtoException {DataTransferObject = this, Exception = e}, EventArgs.Empty);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null) return;
            this.Clean = false;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
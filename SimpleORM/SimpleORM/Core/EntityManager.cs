namespace SimpleORM.Core
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using SimpleORM.Contracts;
    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Linq;
    using SimpleORM.Attributes;

    public class EntityManager : IDbContext
    {
        private SqlConnection connection;
        private string connectionString;
        private bool isCodeFirst;

        public EntityManager(string connectionString, bool isCodeFirst)
        {
            this.connectionString = connectionString;
            this.isCodeFirst = isCodeFirst;
            this.connection = new SqlConnection();
        }

        private object GetId(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Value was not inserted");
            }

            var id = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(IdAttribute)))
                .Select(v => v.GetValue(entity))
                .FirstOrDefault();

            if (id == null)
            {
                throw new ArgumentNullException("Field ID was not found");
            }

            return id;
        }

        private string GetTableName(Type entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Value was not inserted");
            }

            if (!entity.IsDefined(typeof(EntityAttribute)))
            {
                throw new ArgumentNullException("Cannot find table name");
            }

            string table = entity.GetCustomAttribute<EntityAttribute>().TableName;

            return table;
        }

        private string GetColumnName(FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("Field column cannot be null");
            }

            if (!field.IsDefined(typeof(ColumnAttribute)))
            {
                return field.Name;
            }

            string column = field.GetCustomAttribute<ColumnAttribute>().ColumnName;
            if (string.IsNullOrEmpty(column))
            {
                throw new ArgumentNullException("Column name is null");
            }

            return column;
        }

        private void CreateTable(Type entity)
        {
            string query = GetTableCreatingString(entity);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = this.connection;
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        }

        private string GetTableCreatingString(Type entity)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"CREATE TABLE {GetTableName(entity)}(");
            stringBuilder.Append($"Id INT PRIMARY KEY IDENTITY(1,1), ");

            string[] columnNames = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(e => e.IsDefined(typeof(ColumnAttribute)))
                .Select(f => this.GetColumnName(f))
                .ToArray();

            FieldInfo[] fields = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(e => e.IsDefined(typeof(ColumnAttribute)))
                .ToArray();

            for (int i = 0; i < fields.Length; i++)
            {
                stringBuilder.Append($"{columnNames[i]} {this.ConvertToDbType(fields[i].FieldType)}, ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }

        private string ConvertToDbType(Type fieldType)
        {
            switch (fieldType.Name)
            {
                case "Int32":
                    return "INT";
                case "String":
                    return "VARCHAR(100)";
                case "Boolean":
                    return "BIT";
                case "DateTime":
                    return "DATETIME";
                case "Double":
                case "Decimal":
                    return "DECIMAL(10,2)";
                default:
                    throw new ArgumentException($"There is no such type as {fieldType.Name}");
            }
        }

        private bool CheckIfTableExists(Type table)
        {
            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                    $"SELECT COUNT(*) FROM sys.sysobjects WHERE [Name] = '{this.GetTableName(table)}' AND [xtype] = 'U'",
                    this.connection);
                int tableCount = (int)command.ExecuteScalar();

                return tableCount > 0;
            }
        }

        public bool Persist(object entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (isCodeFirst && !CheckIfTableExists(entity.GetType()))
            {
                this.CreateTable(entity.GetType());
            }

            var value = this.GetId(entity);

            if (value == null || (int)value <= 0)
            {
                return this.Insert(entity);
            }

            return this.Update(entity);
        }

        private bool Insert<T>(T entity)
        {
            string query = this.GetInsertString(entity);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, this.connection);
                int rowsAffected = (int)command.ExecuteNonQuery();

                SqlCommand getIdCommand = new SqlCommand(
                    $"SELECT TOP 1 tbl.Id FROM {this.GetTableName(entity.GetType())} tbl ORDER BY tbl.Id DESC",
                    this.connection);
                int id = (int)getIdCommand.ExecuteScalar();

                entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.IsDefined(typeof(IdAttribute)))
                .SetValue(entity, id);

                return rowsAffected > 0;
            }
        }

        private string GetInsertString(object entity)
        {
            string[] columnNames = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(e => e.IsDefined(typeof(ColumnAttribute)))
                .Select(f => GetColumnName(f))
                .ToArray();

            object[] values = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(e => e.IsDefined(typeof(ColumnAttribute)))
                .Select(v => v.GetValue(entity))
                .ToArray();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"INSERT INTO {GetTableName(entity.GetType())}(");

            for (int i = 0; i < columnNames.Length; i++)
            {
                stringBuilder.Append($"{columnNames[i]}, ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(") ");

            stringBuilder.Append("VALUES(");

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].GetType().Name != "Int32")
                {
                    stringBuilder.Append($"'{values[i]}', ");
                }
                else
                {
                    stringBuilder.Append($"{values[i]}, ");
                }
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }

        private bool Update<T>(T entity)
        {
            string query = GetUpdateString(entity);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, this.connection);
                int result = command.ExecuteNonQuery();

                return result > 0;
            }
        }

        private string GetUpdateString(object entity)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"UPDATE {this.GetTableName(entity.GetType())} SET ");

            string[] columnNames = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(ColumnAttribute)))
                .Select(n => GetColumnName(n))
                .ToArray();
            object[] newValues = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(ColumnAttribute)))
                .Select(v => v.GetValue(entity))
                .ToArray();

            for (int i = 0; i < newValues.Length; i++)
            {
                if (newValues[i].GetType().Name == "Int32")
                {
                    stringBuilder.Append($"{columnNames[i]}={newValues[i]}, ");
                }
                else
                {
                    stringBuilder.Append($"{columnNames[i]}='{newValues[i]}', ");
                }
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);

            stringBuilder.Append($" Where Id = {this.GetId(entity)}");

            return stringBuilder.ToString();
        }

        public T FindById<T>(int id)
        {
            T result = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    $"SELECT * FROM {this.GetTableName(typeof(T))} WHERE Id = @Id",
                    this.connection);
                command.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("No entity was found with id " + id);
                    }

                    reader.Read();
                    result = CreateEntity<T>(reader);
                }
            }

            return result;
        }

        private T CreateEntity<T>(SqlDataReader reader)
        {
            object[] originalValues = new object[reader.FieldCount];
            reader.GetValues(originalValues);

            object[] values = new object[reader.FieldCount - 1];
            Array.Copy(originalValues, 1, values, 0, reader.FieldCount - 1);
            Type[] types = new Type[values.Length];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = values[i].GetType();
            }

            T entity = (T)typeof(T).GetConstructor(types).Invoke(values);
            typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.IsDefined(typeof(IdAttribute)))
                .SetValue(entity, originalValues[0]);

            return entity;
        }

        public IEnumerable<T> FindAll<T>()
        {
            IEnumerable<T> list = new List<T>();
            T user = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                   $"SELECT * FROM {this.GetTableName(typeof(T))}",
                   this.connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("There is no such table in database");
                    }

                    while (reader.Read())
                    {
                        user = CreateEntity<T>(reader);
                        list = list.Concat(new[] {user});
                    }
                }

                return list;
            }
        }

        public IEnumerable<T> FindAll<T>(string condition)
        {
            IEnumerable<T> list = new List<T>();
            T user = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(
                   $"SELECT * FROM {this.GetTableName(typeof(T))} WHERE {condition}",
                   this.connection);
                command.Parameters.AddWithValue("@Condition", condition);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("There is no such column/value in table");
                    }

                    while (reader.Read())
                    {
                        user = CreateEntity<T>(reader);
                        list = list.Concat(new[] { user });
                    }
                }

                return list;
            }
        }

        public T FindFirst<T>()
        {
            T result = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    $"SELECT TOP 1 * FROM {this.GetTableName(typeof(T))}",
                    this.connection);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("Empty database");
                    }

                    reader.Read();
                    result = CreateEntity<T>(reader);
                }
            }

            return result;
        }

        public T FindFirst<T>(string condition)
        {
            T result = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    $"SELECT TOP 1 * FROM {this.GetTableName(typeof(T))} WHERE {condition}",
                    this.connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("No entity was found with this " + condition);
                    }

                    reader.Read();
                    result = CreateEntity<T>(reader);
                }
            }

            return result;
        }

        public void Delete<T>(object entity)
        {
            var firstOrDefault = entity.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.IsDefined(typeof(IdAttribute)));

            if (firstOrDefault == null)
            {
                throw new NullReferenceException("Entity does not have field with Id attribute");
            }

            this.DeleteById<T>((int) firstOrDefault.GetValue(entity));
        }

        public void DeleteById<T>(int id)
        {
            string query = $"DELETE FROM {this.GetTableName(typeof(T))} WHERE ID = @id";

            using (this.connection = new SqlConnection(this.connectionString))
            {
                this.connection.Open();

                SqlCommand command = new SqlCommand(query, this.connection);
                command.Parameters.AddWithValue("@id", id);
                int result = command.ExecuteNonQuery();

                if (result == 0)
                {
                    throw new ArgumentException($"Id {id} not found");
                }
            }
        }
    }
}

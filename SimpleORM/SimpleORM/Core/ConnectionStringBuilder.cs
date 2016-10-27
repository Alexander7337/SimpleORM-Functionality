namespace SimpleORM.Core
{
    using System;
    using System.Data.SqlClient;
    public class ConnectionStringBuilder
    {
        private SqlConnectionStringBuilder connectionStringBuilder;

        private string connectionString;
        public ConnectionStringBuilder(string databaseName)
        {
            this.connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder["Data Source"] = "(local)";
            connectionStringBuilder["Integrated Security"] = true;
            connectionStringBuilder["Connect Timeout"] = 1000;
            connectionStringBuilder["Trusted_Connection"] = true;
            connectionStringBuilder["Initial Catalog"] = databaseName;
            this.connectionString = connectionStringBuilder.ToString();
        }

        public string ConnectionString => this.connectionString;
    }
}

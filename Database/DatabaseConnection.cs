using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Database
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string databaseFilePath = null;
        public SqliteConnection Connection { get; private set; }

        public DatabaseConnection(string databaseFilePath, bool autopen = true)
        {
            this.databaseFilePath = databaseFilePath;

            if (autopen)
            {
                this.Open();
            }
        }

        public SqliteConnection Open()
        {
            SqliteConnectionStringBuilder connBuilder = new()
            {
                DataSource = Path.Combine(this.databaseFilePath)
            };

            this.Connection = new SqliteConnection(connBuilder.ToString());
            this.Connection.Open();

            return this.Connection;
        }

        public void Close()
        {
            this.Connection?.Close();
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            this.Connection?.Close();
            this.Connection?.Dispose();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}

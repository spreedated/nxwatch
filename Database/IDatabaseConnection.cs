using Microsoft.Data.Sqlite;
using System;

namespace Database
{
    public interface IDatabaseConnection : IDisposable
    {
        public SqliteConnection Connection { get; }
        public SqliteConnection Open();
        public void Close();
    }
}

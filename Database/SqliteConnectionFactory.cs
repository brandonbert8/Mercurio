using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using Microsoft.Data.Sqlite;
namespace Mercurio.src.Database
{
    
    class SqliteConnectionFactory
    {
        public readonly DatabaseConfiguration _configuration;
        public SqliteConnectionFactory(DatabaseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqliteConnection CreateConnection()
        {
            var connectionString = $"Data Source={_configuration.DatabasePath}";
            return new SqliteConnection(connectionString);
        }
    }
}

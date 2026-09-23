using System;
using System.Collections.Generic;
using System.Text;

namespace Mercurio.src.Database
{
    class DatabaseConfiguration
    {
        public string DataDirectory { get; }
        public string DatabasePath { get; }

        public DatabaseConfiguration()
        {
            var applicationDirectory = AppContext.BaseDirectory;
            DataDirectory = System.IO.Path.Combine(applicationDirectory, "Data");
            DatabasePath = System.IO.Path.Combine(DataDirectory, "mercurio.db");

        }
    }
}

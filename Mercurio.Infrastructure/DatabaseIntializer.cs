using DbUp;
using System;
using System.Reflection;

namespace Mercurio.Infrastructure
{
    public static class DatabaseInitializer
    {
        public static void Initialize(string connectionString)
        {
            var upgrader = DeployChanges.To
                .SqliteDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToNowhere()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new Exception("Error al inicializar la base de datos SQLite.", result.Error);
            }
        }
    }
}
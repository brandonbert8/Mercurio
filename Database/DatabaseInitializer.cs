using System;
using System.Collections.Generic;
using System.IO;
using Dapper;

namespace Mercurio.src.Database
{
    
    class DatabaseInitializer
    {
        public readonly DatabaseConfiguration _configuration;
        public readonly SqliteConnectionFactory _connectionFactory;

        public DatabaseInitializer(DatabaseConfiguration configuration, SqliteConnectionFactory connectionFactory)
        {
            _configuration = configuration;
            _connectionFactory = connectionFactory;
        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("=================================");
            Console.WriteLine("Mercurio - Database Initialization");
            Console.WriteLine("=================================");

            Console.WriteLine(
                $"Data directory: {_configuration.DataDirectory}");

            Console.WriteLine(
                $"Database: {_configuration.DatabasePath}");

            Directory.CreateDirectory(
                _configuration.DataDirectory);

            Console.WriteLine("Opening SQLite connection...");

            using var connection =
                _connectionFactory.CreateConnection();

            await connection.OpenAsync();

            Console.WriteLine("SQLite connection opened.");

            await CreateMigrationTableAsync(connection);

            await RunMigrationsAsync(connection);

            Console.WriteLine("Database initialization completed.");
            Console.WriteLine("=================================");
        }
        private static async Task CreateMigrationTableAsync(
        System.Data.Common.DbConnection connection)
        {
            Console.WriteLine(
                "Checking schema_migrations table...");

            await connection.ExecuteAsync(
                """
            CREATE TABLE IF NOT EXISTS schema_migrations
            (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                applied_at TEXT NOT NULL
            );
            """
            );

            Console.WriteLine(
                "schema_migrations is ready.");
        }
        private static async Task RunMigrationsAsync(
        System.Data.Common.DbConnection connection)
        {
            await connection.ExecuteAsync(
                """
            CREATE TABLE IF NOT EXISTS schema_migrations
            (
                version INTEGER PRIMARY KEY,
                name TEXT NOT NULL,
                applied_at TEXT NOT NULL
            );
            """
            );

            var migrationDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "Database",
                "Migrations"
            );

            if (!Directory.Exists(migrationDirectory))
                throw new DirectoryNotFoundException(
                    $"Migration directory not found: {migrationDirectory}"
                );

            var migrationFiles = Directory
                .GetFiles(migrationDirectory, "*.sql")
                .OrderBy(Path.GetFileName)
                .ToList();
            foreach (var migrationFile in migrationFiles)
            {
                var fileName =
                    Path.GetFileNameWithoutExtension(
                        migrationFile
                    );

                var parts = fileName.Split('_', 2);

                if (parts.Length != 2 ||
                    !int.TryParse(parts[0], out var version))
                {
                    throw new InvalidOperationException(
                        $"Invalid migration filename: {fileName}"
                    );
                }

                var alreadyApplied =
                    await connection.ExecuteScalarAsync<bool>(
                        """
                    SELECT EXISTS(
                        SELECT 1
                        FROM schema_migrations
                        WHERE version = @Version
                    );
                    """,
                        new { Version = version }
                    );

                if (alreadyApplied)
                    continue;

                var sql =
                    await File.ReadAllTextAsync(
                        migrationFile
                    );

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    await connection.ExecuteAsync(
                        sql,
                        transaction: transaction
                    );

                    await connection.ExecuteAsync(
                        """
                    INSERT INTO schema_migrations
                    (
                        version,
                        name,
                        applied_at
                    )
                    VALUES
                    (
                        @Version,
                        @Name,
                        @AppliedAt
                    );
                    """,
                        new
                        {
                            Version = version,
                            Name = fileName,
                            AppliedAt =
                                DateTime.UtcNow
                                    .ToString("O")
                        },
                        transaction
                    );

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            }

        }
    }

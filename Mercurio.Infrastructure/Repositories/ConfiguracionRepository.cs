using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class ConfiguracionRepository : IConfiguracionRepository
    {
        private readonly string _connectionString;

        public ConfiguracionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Configuracion>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Configuracion>(
                "SELECT * FROM Configuraciones WHERE NegocioId = @NegocioId ORDER BY Grupo, Clave",
                new { NegocioId = negocioId });
        }

        public async Task<IEnumerable<Configuracion>> GetByGrupoAsync(string negocioId, string grupo)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Configuracion>(
                "SELECT * FROM Configuraciones WHERE NegocioId = @NegocioId AND Grupo = @Grupo ORDER BY Clave",
                new { NegocioId = negocioId, Grupo = grupo });
        }

        public async Task<Configuracion?> GetByClaveAsync(string negocioId, string clave)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Configuracion>(
                "SELECT * FROM Configuraciones WHERE NegocioId = @NegocioId AND Clave = @Clave",
                new { NegocioId = negocioId, Clave = clave });
        }

        public async Task UpsertAsync(Configuracion configuracion)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Configuraciones (Id, NegocioId, Clave, Valor, Descripcion, Grupo, FechaActualizacion)
                VALUES (@Id, @NegocioId, @Clave, @Valor, @Descripcion, @Grupo, @FechaActualizacion)
                ON CONFLICT(NegocioId, Clave) DO UPDATE SET
                    Valor = excluded.Valor,
                    Descripcion = excluded.Descripcion,
                    Grupo = excluded.Grupo,
                    FechaActualizacion = excluded.FechaActualizacion";
            await connection.ExecuteAsync(sql, configuracion);
        }

        public async Task EliminarAsync(string negocioId, string clave)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "DELETE FROM Configuraciones WHERE NegocioId = @NegocioId AND Clave = @Clave",
                new { NegocioId = negocioId, Clave = clave });
        }
    }
}

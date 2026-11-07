using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class NegocioRepository : INegocioRepository
    {
        private readonly string _connectionString;

        public NegocioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Negocio>> GetAllAsync()
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Negocio>("SELECT * FROM Negocios WHERE Activo = 1 ORDER BY Nombre");
        }

        public async Task<Negocio?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Negocio>("SELECT * FROM Negocios WHERE Id = @Id", new { Id = id });
        }

        public async Task InsertarAsync(Negocio negocio)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Negocios (Id, Nombre, RazonSocial, Ruc, Direccion, Telefono, Email, Moneda, ZonaHoraria, Activo, FechaCreacion)
                VALUES (@Id, @Nombre, @RazonSocial, @Ruc, @Direccion, @Telefono, @Email, @Moneda, @ZonaHoraria, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, negocio);
        }

        public async Task ActualizarAsync(Negocio negocio)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Negocios SET
                    Nombre = @Nombre, RazonSocial = @RazonSocial, Ruc = @Ruc, Direccion = @Direccion,
                    Telefono = @Telefono, Email = @Email, Moneda = @Moneda, ZonaHoraria = @ZonaHoraria, Activo = @Activo
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, negocio);
        }
    }

    public class SucursalRepository : ISucursalRepository
    {
        private readonly string _connectionString;

        public SucursalRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Sucursal>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Sucursal>(
                "SELECT * FROM Sucursales WHERE NegocioId = @NegocioId ORDER BY Nombre",
                new { NegocioId = negocioId });
        }

        public async Task<Sucursal?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Sucursal>("SELECT * FROM Sucursales WHERE Id = @Id", new { Id = id });
        }

        public async Task InsertarAsync(Sucursal sucursal)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Sucursales (Id, NegocioId, Nombre, Codigo, Direccion, Telefono, Activa, FechaCreacion)
                VALUES (@Id, @NegocioId, @Nombre, @Codigo, @Direccion, @Telefono, @Activa, @FechaCreacion)";
            await connection.ExecuteAsync(sql, sucursal);
        }

        public async Task ActualizarAsync(Sucursal sucursal)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Sucursales SET Nombre = @Nombre, Codigo = @Codigo, Direccion = @Direccion,
                    Telefono = @Telefono, Activa = @Activa
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, sucursal);
        }
    }

    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Usuario>(
                "SELECT * FROM Usuarios WHERE NegocioId = @NegocioId ORDER BY NombreCompleto",
                new { NegocioId = negocioId });
        }

        public async Task<Usuario?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM Usuarios WHERE Id = @Id", new { Id = id });
        }

        public async Task<Usuario?> GetByNombreUsuarioAsync(string negocioId, string nombreUsuario)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = "SELECT * FROM Usuarios WHERE NegocioId = @NegocioId AND NombreUsuario = @NombreUsuario";
            return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { NegocioId = negocioId, NombreUsuario = nombreUsuario });
        }

        public async Task InsertarAsync(Usuario usuario)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Usuarios (Id, NegocioId, SucursalId, NombreUsuario, NombreCompleto, Email, PasswordHash,
                    PasswordSalt, Rol, Activo, FechaCreacion, UltimoAcceso)
                VALUES (@Id, @NegocioId, @SucursalId, @NombreUsuario, @NombreCompleto, @Email, @PasswordHash,
                    @PasswordSalt, @Rol, @Activo, @FechaCreacion, @UltimoAcceso)";
            await connection.ExecuteAsync(sql, usuario);
        }

        public async Task ActualizarAsync(Usuario usuario)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Usuarios SET
                    SucursalId = @SucursalId, NombreUsuario = @NombreUsuario, NombreCompleto = @NombreCompleto,
                    Email = @Email, PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt,
                    Rol = @Rol, Activo = @Activo, UltimoAcceso = @UltimoAcceso
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, usuario);
        }

        public async Task ActualizarUltimoAccesoAsync(string id, DateTime fecha)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Usuarios SET UltimoAcceso = @Fecha WHERE Id = @Id", new { Id = id, Fecha = fecha });
        }
    }
}

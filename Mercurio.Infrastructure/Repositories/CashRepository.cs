using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class CashRepository : ICashRepository
    {
        private readonly string _connectionString;

        public CashRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Caja>> GetCajasAsync(string sucursalId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Caja>(
                "SELECT * FROM Cajas WHERE SucursalId = @SucursalId ORDER BY Nombre",
                new { SucursalId = sucursalId });
        }

        public async Task<Caja?> GetCajaByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Caja>("SELECT * FROM Cajas WHERE Id = @Id", new { Id = id });
        }

        public async Task InsertarCajaAsync(Caja caja)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Cajas (Id, SucursalId, Nombre, Activa, FechaCreacion)
                VALUES (@Id, @SucursalId, @Nombre, @Activa, @FechaCreacion)";
            await connection.ExecuteAsync(sql, caja);
        }

        public async Task ActualizarCajaAsync(Caja caja)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE Cajas SET Nombre = @Nombre, Activa = @Activa WHERE Id = @Id",
                caja);
        }

        public async Task<CajaSesion?> GetSesionByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CajaSesion>(
                "SELECT * FROM CajaSesiones WHERE Id = @Id", new { Id = id });
        }

        public async Task<CajaSesion?> GetSesionAbiertaAsync(string cajaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CajaSesion>(
                "SELECT * FROM CajaSesiones WHERE CajaId = @CajaId AND Estado = @Estado ORDER BY FechaApertura DESC",
                new { CajaId = cajaId, Estado = (int)EstadoCajaSesion.Abierta });
        }

        public async Task<IEnumerable<CajaSesion>> GetSesionesAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT s.* FROM CajaSesiones s
                INNER JOIN Cajas c ON c.Id = s.CajaId
                WHERE c.SucursalId = @SucursalId AND s.FechaApertura >= @Desde AND s.FechaApertura <= @Hasta
                ORDER BY s.FechaApertura DESC";
            return await connection.QueryAsync<CajaSesion>(sql, new { SucursalId = sucursalId, Desde = desde, Hasta = hasta });
        }

        public async Task InsertarSesionAsync(CajaSesion sesion)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO CajaSesiones (Id, CajaId, UsuarioId, FechaApertura, FechaCierre, MontoInicial,
                    MontoEsperado, MontoDeclarado, Diferencia, Estado, Observacion)
                VALUES (@Id, @CajaId, @UsuarioId, @FechaApertura, @FechaCierre, @MontoInicial,
                    @MontoEsperado, @MontoDeclarado, @Diferencia, @Estado, @Observacion)";
            await connection.ExecuteAsync(sql, sesion);
        }

        public async Task ActualizarSesionAsync(CajaSesion sesion)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE CajaSesiones SET FechaCierre = @FechaCierre, MontoEsperado = @MontoEsperado,
                    MontoDeclarado = @MontoDeclarado, Diferencia = @Diferencia, Estado = @Estado, Observacion = @Observacion
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, sesion);
        }

        public async Task InsertarMovimientoAsync(CajaMovimiento movimiento)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO CajaMovimientos (Id, CajaSesionId, Tipo, Concepto, Monto, Fecha, UsuarioId, Referencia)
                VALUES (@Id, @CajaSesionId, @Tipo, @Concepto, @Monto, @Fecha, @UsuarioId, @Referencia)";
            await connection.ExecuteAsync(sql, movimiento);
        }

        public async Task<IEnumerable<CajaMovimiento>> GetMovimientosAsync(string cajaSesionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<CajaMovimiento>(
                "SELECT * FROM CajaMovimientos WHERE CajaSesionId = @CajaSesionId ORDER BY Fecha",
                new { CajaSesionId = cajaSesionId });
        }

        public async Task<decimal> CalcularMontoSistemaAsync(string cajaSesionId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT COALESCE(s.MontoInicial, 0) + COALESCE((
                    SELECT SUM(CASE
                        WHEN m.Tipo IN (@Ingreso, @Venta, @Cobro) THEN m.Monto
                        WHEN m.Tipo = @Egreso THEN -m.Monto
                        ELSE 0
                    END)
                    FROM CajaMovimientos m
                    WHERE m.CajaSesionId = s.Id
                ), 0)
                FROM CajaSesiones s
                WHERE s.Id = @CajaSesionId";
            return await connection.ExecuteScalarAsync<decimal>(sql, new
            {
                CajaSesionId = cajaSesionId,
                Ingreso = (int)TipoMovimientoCaja.Ingreso,
                Venta = (int)TipoMovimientoCaja.Venta,
                Cobro = (int)TipoMovimientoCaja.Cobro,
                Egreso = (int)TipoMovimientoCaja.Egreso
            });
        }
    }
}

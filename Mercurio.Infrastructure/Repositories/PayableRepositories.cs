using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class CuentaPorPagarRepository : ICuentaPorPagarRepository
    {
        private readonly string _connectionString;

        public CuentaPorPagarRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<CuentaPorPagar?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CuentaPorPagar>(
                "SELECT * FROM CuentasPorPagar WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<CuentaPorPagar>> GetByProveedorAsync(string proveedorId, bool soloPendientes = false)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM CuentasPorPagar WHERE ProveedorId = @ProveedorId";
            if (soloPendientes)
                sql += " AND Estado IN (1, 2)";
            sql += " ORDER BY FechaVencimiento";

            return await connection.QueryAsync<CuentaPorPagar>(sql, new { ProveedorId = proveedorId });
        }

        public async Task<IEnumerable<CuentaPorPagar>> GetPendientesAsync(string negocioId, DateTime? fechaCorte = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT c.* FROM CuentasPorPagar c
                INNER JOIN Proveedores p ON p.Id = c.ProveedorId
                WHERE p.NegocioId = @NegocioId AND c.Estado IN (1, 2)
                  AND (@FechaCorte IS NULL OR date(c.FechaVencimiento) <= date(@FechaCorte))
                ORDER BY c.FechaVencimiento";
            return await connection.QueryAsync<CuentaPorPagar>(sql, new { NegocioId = negocioId, FechaCorte = fechaCorte });
        }

        public async Task<IEnumerable<CuentaPorPagar>> GetVencidasAsync(string negocioId, DateTime fechaCorte)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT c.* FROM CuentasPorPagar c
                INNER JOIN Proveedores p ON p.Id = c.ProveedorId
                WHERE p.NegocioId = @NegocioId AND c.Estado IN (1, 2)
                  AND date(c.FechaVencimiento) < date(@FechaCorte)
                ORDER BY c.FechaVencimiento";
            return await connection.QueryAsync<CuentaPorPagar>(sql, new { NegocioId = negocioId, FechaCorte = fechaCorte });
        }

        public async Task<IEnumerable<AbonoPago>> GetAbonosAsync(string cuentaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<AbonoPago>(
                "SELECT * FROM AbonosPago WHERE CuentaPorPagarId = @CuentaId ORDER BY Fecha",
                new { CuentaId = cuentaId });
        }

        public async Task<decimal> GetSaldoProveedorAsync(string proveedorId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT COALESCE(SUM(Saldo), 0) FROM CuentasPorPagar
                WHERE ProveedorId = @ProveedorId AND Estado IN (1, 2) AND Saldo > 0";
            return await connection.ExecuteScalarAsync<decimal>(sql, new { ProveedorId = proveedorId });
        }

        public async Task RegistrarAbonoAsync(
            AbonoPago abono,
            decimal saldoAnterior,
            decimal nuevoSaldo,
            int estado,
            CajaMovimiento? movimientoCaja = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(@"
                INSERT INTO AbonosPago (Id, CuentaPorPagarId, ProveedorId, Fecha, Monto, MetodoPago,
                    Referencia, UsuarioId, CajaSesionId, Observacion)
                VALUES (@Id, @CuentaPorPagarId, @ProveedorId, @Fecha, @Monto, @MetodoPago,
                    @Referencia, @UsuarioId, @CajaSesionId, @Observacion)",
                abono,
                transaction);

            var affected = await connection.ExecuteAsync(
                "UPDATE CuentasPorPagar SET Saldo = @Saldo, Estado = @Estado WHERE Id = @Id AND Saldo = @SaldoAnterior",
                new
                {
                    Saldo = nuevoSaldo,
                    Estado = estado,
                    Id = abono.CuentaPorPagarId,
                    SaldoAnterior = saldoAnterior
                },
                transaction);

            if (affected == 0)
                throw new InvalidOperationException("La cuenta cambió mientras se registraba el pago. Intente nuevamente.");

            if (movimientoCaja is not null)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO CajaMovimientos (Id, CajaSesionId, Tipo, Concepto, Monto, Fecha, UsuarioId, Referencia)
                    VALUES (@Id, @CajaSesionId, @Tipo, @Concepto, @Monto, @Fecha, @UsuarioId, @Referencia)",
                    movimientoCaja,
                    transaction);
            }

            transaction.Commit();
        }

        public async Task ActualizarEstadoAsync(string cuentaId, int estado)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE CuentasPorPagar SET Estado = @Estado WHERE Id = @Id",
                new { Estado = estado, Id = cuentaId });
        }
    }
}

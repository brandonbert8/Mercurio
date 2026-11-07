using Dapper;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly string _connectionString;

        public ClienteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync(string negocioId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Cliente>(
                "SELECT * FROM Clientes WHERE NegocioId = @NegocioId ORDER BY Nombre",
                new { NegocioId = negocioId });
        }

        public async Task<Cliente?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cliente>("SELECT * FROM Clientes WHERE Id = @Id", new { Id = id });
        }

        public async Task<Cliente?> GetByDocumentoAsync(string negocioId, string numeroDocumento)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cliente>(
                "SELECT * FROM Clientes WHERE NegocioId = @NegocioId AND NumeroDocumento = @NumeroDocumento",
                new { NegocioId = negocioId, NumeroDocumento = numeroDocumento });
        }

        public async Task<IEnumerable<Cliente>> BuscarAsync(string negocioId, string termino)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Clientes
                WHERE NegocioId = @NegocioId
                  AND (Nombre LIKE @Termino OR NumeroDocumento LIKE @Termino OR Telefono LIKE @Termino OR Email LIKE @Termino)
                ORDER BY Nombre";
            return await connection.QueryAsync<Cliente>(sql, new { NegocioId = negocioId, Termino = $"%{termino}%" });
        }

        public async Task InsertarAsync(Cliente cliente)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO Clientes (Id, NegocioId, Nombre, TipoDocumento, NumeroDocumento, Telefono, Email,
                    Direccion, LimiteCredito, Activo, FechaCreacion)
                VALUES (@Id, @NegocioId, @Nombre, @TipoDocumento, @NumeroDocumento, @Telefono, @Email,
                    @Direccion, @LimiteCredito, @Activo, @FechaCreacion)";
            await connection.ExecuteAsync(sql, cliente);
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                UPDATE Clientes SET Nombre = @Nombre, TipoDocumento = @TipoDocumento, NumeroDocumento = @NumeroDocumento,
                    Telefono = @Telefono, Email = @Email, Direccion = @Direccion,
                    LimiteCredito = @LimiteCredito, Activo = @Activo
                WHERE Id = @Id";
            await connection.ExecuteAsync(sql, cliente);
        }

        public async Task DesactivarAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("UPDATE Clientes SET Activo = 0 WHERE Id = @Id", new { Id = id });
        }
    }

    public class CreditoRepository : ICreditoRepository
    {
        private readonly string _connectionString;

        public CreditoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<CuentaPorCobrar?> GetByIdAsync(string id)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<CuentaPorCobrar>(
                "SELECT * FROM CuentasPorCobrar WHERE Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<CuentaPorCobrar>> GetByClienteAsync(string clienteId, bool soloPendientes = false)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = "SELECT * FROM CuentasPorCobrar WHERE ClienteId = @ClienteId";
            if (soloPendientes)
                sql += " AND Estado IN (1, 2, 4)";
            sql += " ORDER BY FechaVencimiento";

            return await connection.QueryAsync<CuentaPorCobrar>(sql, new { ClienteId = clienteId });
        }

        public async Task<IEnumerable<CuentaPorCobrar>> GetPendientesAsync(string negocioId, DateTime? fechaCorte = null)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT c.* FROM CuentasPorCobrar c
                INNER JOIN Clientes cl ON cl.Id = c.ClienteId
                WHERE cl.NegocioId = @NegocioId AND c.Estado IN (1, 2, 4)
                  AND (@FechaCorte IS NULL OR date(c.FechaVencimiento) <= date(@FechaCorte))
                ORDER BY c.FechaVencimiento";
            return await connection.QueryAsync<CuentaPorCobrar>(sql, new { NegocioId = negocioId, FechaCorte = fechaCorte });
        }

        public async Task<IEnumerable<CuentaPorCobrar>> GetVencidasAsync(string negocioId, DateTime fechaCorte)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT c.* FROM CuentasPorCobrar c
                INNER JOIN Clientes cl ON cl.Id = c.ClienteId
                WHERE cl.NegocioId = @NegocioId AND c.Estado IN (1, 2, 4)
                  AND date(c.FechaVencimiento) < date(@FechaCorte)
                ORDER BY c.FechaVencimiento";
            return await connection.QueryAsync<CuentaPorCobrar>(sql, new { NegocioId = negocioId, FechaCorte = fechaCorte });
        }

        public async Task<IEnumerable<AbonoCobro>> GetAbonosAsync(string cuentaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<AbonoCobro>(
                "SELECT * FROM AbonosCobro WHERE CuentaPorCobrarId = @CuentaId ORDER BY Fecha",
                new { CuentaId = cuentaId });
        }

        public async Task<decimal> GetSaldoClienteAsync(string clienteId)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                SELECT COALESCE(SUM(Saldo), 0) FROM CuentasPorCobrar
                WHERE ClienteId = @ClienteId AND Estado <> @Anulada AND Saldo > 0";
            return await connection.ExecuteScalarAsync<decimal>(sql, new { ClienteId = clienteId, Anulada = (int)EstadoCuentaCobrar.Anulada });
        }

        public async Task CrearAsync(CuentaPorCobrar cuenta)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = @"
                INSERT INTO CuentasPorCobrar (Id, ClienteId, VentaId, FechaEmision, FechaVencimiento,
                    MontoOriginal, Saldo, Estado, Observacion)
                VALUES (@Id, @ClienteId, @VentaId, @FechaEmision, @FechaVencimiento,
                    @MontoOriginal, @Saldo, @Estado, @Observacion)";
            await connection.ExecuteAsync(sql, cuenta);
        }

        public async Task RegistrarAbonoAsync(AbonoCobro abono, decimal nuevoSaldo, int estado)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(@"
                INSERT INTO AbonosCobro (Id, CuentaPorCobrarId, ClienteId, Fecha, Monto, MetodoPago,
                    Referencia, UsuarioId, CajaSesionId, Observacion)
                VALUES (@Id, @CuentaPorCobrarId, @ClienteId, @Fecha, @Monto, @MetodoPago,
                    @Referencia, @UsuarioId, @CajaSesionId, @Observacion)",
                abono,
                transaction);

            await connection.ExecuteAsync(
                "UPDATE CuentasPorCobrar SET Saldo = @Saldo, Estado = @Estado WHERE Id = @Id",
                new { Saldo = nuevoSaldo, Estado = estado, Id = abono.CuentaPorCobrarId },
                transaction);

            transaction.Commit();
        }

        public async Task ActualizarEstadoAsync(string cuentaId, int estado)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE CuentasPorCobrar SET Estado = @Estado WHERE Id = @Id",
                new { Estado = estado, Id = cuentaId });
        }

        public async Task AnularPorVentaAsync(string ventaId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(
                "UPDATE CuentasPorCobrar SET Estado = @Estado, Saldo = 0 WHERE VentaId = @VentaId AND Estado <> @Estado",
                new { Estado = (int)EstadoCuentaCobrar.Anulada, VentaId = ventaId });
        }
    }
}

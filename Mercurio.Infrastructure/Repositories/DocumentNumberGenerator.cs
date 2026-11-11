using Dapper;
using Mercurio.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace Mercurio.Infrastructure.Repositories
{
    internal static class DocumentNumberGenerator
    {
        public static async Task<string> GenerarAsync(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string sucursalId,
            TipoDocumento tipoDocumento,
            DateTime fecha)
        {
            var anio = fecha.Year;
            var ultimo = await connection.ExecuteScalarAsync<int>(@"
                INSERT INTO SecuenciasDocumento (Id, SucursalId, TipoDocumento, Anio, UltimoNumero, FechaActualizacion)
                VALUES (@Id, @SucursalId, @TipoDocumento, @Anio, 1, @FechaActualizacion)
                ON CONFLICT(SucursalId, TipoDocumento, Anio) DO UPDATE SET
                    UltimoNumero = UltimoNumero + 1,
                    FechaActualizacion = excluded.FechaActualizacion
                RETURNING UltimoNumero",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    SucursalId = sucursalId,
                    TipoDocumento = (int)tipoDocumento,
                    Anio = anio,
                    FechaActualizacion = DateTime.Now
                },
                transaction);

            return $"{Prefijo(tipoDocumento)}-{anio}-{ultimo:000000}";
        }

        private static string Prefijo(TipoDocumento tipoDocumento)
        {
            return tipoDocumento switch
            {
                TipoDocumento.Ticket => "T",
                TipoDocumento.Boleta => "B",
                TipoDocumento.Factura => "F",
                TipoDocumento.NotaCredito => "NC",
                TipoDocumento.ComprobanteCompra => "C",
                _ => "D"
            };
        }
    }
}

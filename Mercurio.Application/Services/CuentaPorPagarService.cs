using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class CuentaPorPagarService : ICuentaPorPagarService
    {
        private readonly ICuentaPorPagarRepository _cuentaRepository;

        public CuentaPorPagarService(ICuentaPorPagarRepository cuentaRepository)
        {
            _cuentaRepository = cuentaRepository;
        }

        public async Task<AbonoPagoResult> RegistrarAbonoAsync(RegistrarAbonoPagoRequest request)
        {
            if (request.Monto <= 0)
                throw new BusinessException("El monto del pago debe ser mayor a cero.");

            var cuenta = await _cuentaRepository.GetByIdAsync(request.CuentaPorPagarId);
            if (cuenta is null)
                throw new BusinessException("La cuenta por pagar no existe.");
            if (cuenta.Estado == EstadoCuentaPagar.Anulada)
                throw new BusinessException("La cuenta por pagar está anulada.");
            if (cuenta.Estado == EstadoCuentaPagar.Pagada || cuenta.Saldo <= 0)
                throw new BusinessException("La cuenta por pagar ya está pagada.");
            if (request.Monto > cuenta.Saldo)
                throw new BusinessException("El pago no puede ser mayor al saldo pendiente.");

            var nuevoSaldo = Money.Round(cuenta.Saldo - request.Monto);
            var estado = nuevoSaldo == 0 ? EstadoCuentaPagar.Pagada : EstadoCuentaPagar.Parcial;

            var abono = new AbonoPago
            {
                CuentaPorPagarId = cuenta.Id,
                ProveedorId = cuenta.ProveedorId,
                Fecha = DateTime.Now,
                Monto = Money.Round(request.Monto),
                MetodoPago = request.MetodoPago,
                Referencia = request.Referencia ?? string.Empty,
                UsuarioId = request.UsuarioId,
                CajaSesionId = request.CajaSesionId,
                Observacion = request.Observacion ?? string.Empty
            };

            CajaMovimiento? movimientoCaja = null;
            if (request.MetodoPago == MetodoPago.Efectivo && !string.IsNullOrWhiteSpace(request.CajaSesionId))
            {
                movimientoCaja = new CajaMovimiento
                {
                    CajaSesionId = request.CajaSesionId,
                    Tipo = TipoMovimientoCaja.Egreso,
                    Concepto = "Pago a proveedor",
                    Monto = abono.Monto,
                    Fecha = abono.Fecha,
                    UsuarioId = request.UsuarioId,
                    Referencia = abono.Referencia
                };
            }

            await _cuentaRepository.RegistrarAbonoAsync(
                abono, cuenta.Saldo, nuevoSaldo, (int)estado, movimientoCaja);

            cuenta.Saldo = nuevoSaldo;
            cuenta.Estado = estado;

            return new AbonoPagoResult { Abono = abono, Cuenta = cuenta };
        }

        public Task<CuentaPorPagar?> GetCuentaAsync(string id)
        {
            return _cuentaRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<CuentaPorPagar>> GetCuentasProveedorAsync(string proveedorId, bool soloPendientes = false)
        {
            return _cuentaRepository.GetByProveedorAsync(proveedorId, soloPendientes);
        }

        public Task<IEnumerable<CuentaPorPagar>> GetCuentasPendientesAsync(string negocioId)
        {
            return _cuentaRepository.GetPendientesAsync(negocioId);
        }

        public Task<IEnumerable<CuentaPorPagar>> GetCuentasVencidasAsync(string negocioId, DateTime fechaCorte)
        {
            return _cuentaRepository.GetVencidasAsync(negocioId, fechaCorte);
        }

        public Task<IEnumerable<AbonoPago>> GetAbonosAsync(string cuentaId)
        {
            return _cuentaRepository.GetAbonosAsync(cuentaId);
        }

        public Task<decimal> GetSaldoProveedorAsync(string proveedorId)
        {
            return _cuentaRepository.GetSaldoProveedorAsync(proveedorId);
        }
    }
}

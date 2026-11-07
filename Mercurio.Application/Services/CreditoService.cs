using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class CreditoService : ICreditoService
    {
        private readonly ICreditoRepository _creditoRepository;
        private readonly ICashRepository _cashRepository;

        public CreditoService(ICreditoRepository creditoRepository, ICashRepository cashRepository)
        {
            _creditoRepository = creditoRepository;
            _cashRepository = cashRepository;
        }

        public async Task<AbonoResult> RegistrarAbonoAsync(RegistrarAbonoRequest request)
        {
            if (request.Monto <= 0)
                throw new BusinessException("El monto del abono debe ser mayor a cero.");

            var cuenta = await _creditoRepository.GetByIdAsync(request.CuentaPorCobrarId);
            if (cuenta is null)
                throw new BusinessException("La cuenta por cobrar no existe.");
            if (cuenta.Estado == EstadoCuentaCobrar.Anulada)
                throw new BusinessException("La cuenta por cobrar está anulada.");
            if (cuenta.Estado == EstadoCuentaCobrar.Pagada || cuenta.Saldo <= 0)
                throw new BusinessException("La cuenta por cobrar ya está pagada.");
            if (request.Monto > cuenta.Saldo)
                throw new BusinessException("El abono no puede ser mayor al saldo pendiente.");

            var nuevoSaldo = Money.Round(cuenta.Saldo - request.Monto);
            var estado = nuevoSaldo == 0 ? EstadoCuentaCobrar.Pagada : EstadoCuentaCobrar.Parcial;

            var abono = new AbonoCobro
            {
                CuentaPorCobrarId = cuenta.Id,
                ClienteId = cuenta.ClienteId,
                Fecha = DateTime.Now,
                Monto = Money.Round(request.Monto),
                MetodoPago = request.MetodoPago,
                Referencia = request.Referencia ?? string.Empty,
                UsuarioId = request.UsuarioId,
                CajaSesionId = request.CajaSesionId,
                Observacion = request.Observacion ?? string.Empty
            };

            await _creditoRepository.RegistrarAbonoAsync(abono, nuevoSaldo, (int)estado);

            if (request.MetodoPago == MetodoPago.Efectivo && !string.IsNullOrWhiteSpace(request.CajaSesionId))
            {
                await _cashRepository.InsertarMovimientoAsync(new CajaMovimiento
                {
                    CajaSesionId = request.CajaSesionId,
                    Tipo = TipoMovimientoCaja.Cobro,
                    Concepto = "Cobro de crédito",
                    Monto = abono.Monto,
                    Fecha = abono.Fecha,
                    UsuarioId = request.UsuarioId,
                    Referencia = abono.Referencia
                });
            }

            cuenta.Saldo = nuevoSaldo;
            cuenta.Estado = estado;

            return new AbonoResult { Abono = abono, Cuenta = cuenta };
        }

        public Task<CuentaPorCobrar?> GetCuentaAsync(string id)
        {
            return _creditoRepository.GetByIdAsync(id);
        }

        public Task<IEnumerable<CuentaPorCobrar>> GetCuentasClienteAsync(string clienteId, bool soloPendientes = false)
        {
            return _creditoRepository.GetByClienteAsync(clienteId, soloPendientes);
        }

        public Task<IEnumerable<CuentaPorCobrar>> GetCuentasPendientesAsync(string negocioId)
        {
            return _creditoRepository.GetPendientesAsync(negocioId);
        }

        public Task<IEnumerable<CuentaPorCobrar>> GetCuentasVencidasAsync(string negocioId, DateTime fechaCorte)
        {
            return _creditoRepository.GetVencidasAsync(negocioId, fechaCorte);
        }

        public Task<IEnumerable<AbonoCobro>> GetAbonosAsync(string cuentaId)
        {
            return _creditoRepository.GetAbonosAsync(cuentaId);
        }

        public Task<decimal> GetSaldoClienteAsync(string clienteId)
        {
            return _creditoRepository.GetSaldoClienteAsync(clienteId);
        }

        public async Task ActualizarVencidasAsync(string negocioId, DateTime fechaCorte)
        {
            var vencidas = await _creditoRepository.GetVencidasAsync(negocioId, fechaCorte);
            foreach (var cuenta in vencidas)
            {
                if (cuenta.Estado != EstadoCuentaCobrar.Vencida)
                    await _creditoRepository.ActualizarEstadoAsync(cuenta.Id, (int)EstadoCuentaCobrar.Vencida);
            }
        }

        public Task AnularCuentaPorVentaAsync(string ventaId)
        {
            return _creditoRepository.AnularPorVentaAsync(ventaId);
        }
    }
}

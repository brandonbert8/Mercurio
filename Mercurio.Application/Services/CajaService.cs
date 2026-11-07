using Mercurio.Application.Common;
using Mercurio.Application.DTOs;
using Mercurio.Application.Interfaces;
using Mercurio.Domain.Entities;
using Mercurio.Domain.Enums;
using Mercurio.Domain.Interfaces;

namespace Mercurio.Application.Services
{
    public class CajaService : ICajaService
    {
        private readonly ICashRepository _cashRepository;

        public CajaService(ICashRepository cashRepository)
        {
            _cashRepository = cashRepository;
        }

        public Task<IEnumerable<Caja>> GetCajasAsync(string sucursalId)
        {
            return _cashRepository.GetCajasAsync(sucursalId);
        }

        public Task<Caja?> GetCajaAsync(string id)
        {
            return _cashRepository.GetCajaByIdAsync(id);
        }

        public async Task<Caja> CrearCajaAsync(Caja caja)
        {
            if (string.IsNullOrWhiteSpace(caja.SucursalId))
                throw new BusinessException("La sucursal es obligatoria.");
            if (string.IsNullOrWhiteSpace(caja.Nombre))
                throw new BusinessException("El nombre de la caja es obligatorio.");

            await _cashRepository.InsertarCajaAsync(caja);
            return caja;
        }

        public async Task ActualizarCajaAsync(Caja caja)
        {
            var existente = await _cashRepository.GetCajaByIdAsync(caja.Id);
            if (existente is null)
                throw new BusinessException("La caja no existe.");
            if (string.IsNullOrWhiteSpace(caja.Nombre))
                throw new BusinessException("El nombre de la caja es obligatorio.");

            await _cashRepository.ActualizarCajaAsync(caja);
        }

        public async Task<CajaSesion> AbrirCajaAsync(AbrirCajaRequest request)
        {
            if (request.MontoInicial < 0)
                throw new BusinessException("El monto inicial no puede ser negativo.");

            var caja = await _cashRepository.GetCajaByIdAsync(request.CajaId);
            if (caja is null || !caja.Activa)
                throw new BusinessException("La caja no existe o está inactiva.");

            var sesionAbierta = await _cashRepository.GetSesionAbiertaAsync(request.CajaId);
            if (sesionAbierta is not null)
                throw new BusinessException("La caja ya tiene una sesión abierta.");

            var sesion = new CajaSesion
            {
                CajaId = request.CajaId,
                UsuarioId = request.UsuarioId,
                FechaApertura = DateTime.Now,
                MontoInicial = Money.Round(request.MontoInicial),
                Estado = EstadoCajaSesion.Abierta,
                Observacion = request.Observacion ?? string.Empty
            };

            await _cashRepository.InsertarSesionAsync(sesion);
            await _cashRepository.InsertarMovimientoAsync(new CajaMovimiento
            {
                CajaSesionId = sesion.Id,
                Tipo = TipoMovimientoCaja.Apertura,
                Concepto = "Apertura de caja",
                Monto = sesion.MontoInicial,
                Fecha = sesion.FechaApertura,
                UsuarioId = request.UsuarioId
            });

            return sesion;
        }

        public async Task<CierreCajaResult> CerrarCajaAsync(CerrarCajaRequest request)
        {
            if (request.MontoDeclarado < 0)
                throw new BusinessException("El monto declarado no puede ser negativo.");

            var sesion = await _cashRepository.GetSesionByIdAsync(request.CajaSesionId);
            if (sesion is null)
                throw new BusinessException("La sesión de caja no existe.");
            if (sesion.Estado == EstadoCajaSesion.Cerrada)
                throw new BusinessException("La sesión de caja ya está cerrada.");

            var movimientos = (await _cashRepository.GetMovimientosAsync(sesion.Id)).ToList();
            var ventasEfectivo = Money.Round(movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Venta).Sum(m => m.Monto));
            var cobrosEfectivo = Money.Round(movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Cobro).Sum(m => m.Monto));
            var ingresos = Money.Round(movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto));
            var egresos = Money.Round(movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto));

            var montoEsperado = await _cashRepository.CalcularMontoSistemaAsync(sesion.Id);
            montoEsperado = Money.Round(montoEsperado);
            var diferencia = Money.Round(request.MontoDeclarado - montoEsperado);

            sesion.FechaCierre = DateTime.Now;
            sesion.MontoEsperado = montoEsperado;
            sesion.MontoDeclarado = Money.Round(request.MontoDeclarado);
            sesion.Diferencia = diferencia;
            sesion.Estado = EstadoCajaSesion.Cerrada;
            sesion.Observacion = request.Observacion ?? string.Empty;

            await _cashRepository.ActualizarSesionAsync(sesion);

            return new CierreCajaResult
            {
                Sesion = sesion,
                VentasEfectivo = ventasEfectivo,
                CobrosEfectivo = cobrosEfectivo,
                Ingresos = ingresos,
                Egresos = egresos,
                MontoEsperado = montoEsperado,
                Diferencia = diferencia
            };
        }

        public Task<CajaSesion?> GetSesionAbiertaAsync(string cajaId)
        {
            return _cashRepository.GetSesionAbiertaAsync(cajaId);
        }

        public Task<CajaSesion?> GetSesionAsync(string id)
        {
            return _cashRepository.GetSesionByIdAsync(id);
        }

        public async Task<CajaMovimiento> RegistrarMovimientoAsync(MovimientoCajaRequest request)
        {
            if (request.Monto <= 0)
                throw new BusinessException("El monto del movimiento debe ser mayor a cero.");
            if (request.Tipo != TipoMovimientoCaja.Ingreso && request.Tipo != TipoMovimientoCaja.Egreso)
                throw new BusinessException("Solo se permiten movimientos de ingreso o egreso manuales.");

            var sesion = await _cashRepository.GetSesionByIdAsync(request.CajaSesionId);
            if (sesion is null)
                throw new BusinessException("La sesión de caja no existe.");
            if (sesion.Estado == EstadoCajaSesion.Cerrada)
                throw new BusinessException("La sesión de caja está cerrada.");

            var movimiento = new CajaMovimiento
            {
                CajaSesionId = request.CajaSesionId,
                Tipo = request.Tipo,
                Concepto = request.Concepto ?? string.Empty,
                Monto = Money.Round(request.Monto),
                Fecha = DateTime.Now,
                UsuarioId = request.UsuarioId,
                Referencia = request.Referencia ?? string.Empty
            };

            await _cashRepository.InsertarMovimientoAsync(movimiento);
            return movimiento;
        }

        public Task<IEnumerable<CajaMovimiento>> GetMovimientosAsync(string cajaSesionId)
        {
            return _cashRepository.GetMovimientosAsync(cajaSesionId);
        }

        public Task<IEnumerable<CajaSesion>> GetSesionesAsync(string sucursalId, DateTime desde, DateTime hasta)
        {
            return _cashRepository.GetSesionesAsync(sucursalId, desde, hasta);
        }
    }
}

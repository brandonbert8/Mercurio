namespace Mercurio.Domain.Calculations
{
    public readonly record struct PrecioDesglosado(decimal BaseImponible, decimal Impuesto, decimal Total)
    {
        public decimal TasaEfectiva =>
            BaseImponible == 0 ? 0m : Impuesto / BaseImponible * 100m;
    }

    public static class PricingCalculator
    {
        public const decimal ImpuestoPorcentajeBolivia = 13m;

        public static decimal Redondear(decimal valor)
        {
            return decimal.Round(valor, 2, MidpointRounding.AwayFromZero);
        }

        public static PrecioDesglosado Desglosar(decimal totalConImpuesto, decimal impuestoPorcentaje)
        {
            if (totalConImpuesto < 0)
                throw new ArgumentOutOfRangeException(nameof(totalConImpuesto), "El total no puede ser negativo.");
            if (impuestoPorcentaje < 0)
                throw new ArgumentOutOfRangeException(nameof(impuestoPorcentaje), "La tasa de impuesto no puede ser negativa.");

            totalConImpuesto = Redondear(totalConImpuesto);
            var factor = 1m + impuestoPorcentaje / 100m;
            var baseImponible = Redondear(totalConImpuesto / factor);
            var impuesto = Redondear(totalConImpuesto - baseImponible);

            return new PrecioDesglosado(baseImponible, impuesto, totalConImpuesto);
        }

        public static PrecioDesglosado DesglosarLinea(
            int cantidad,
            decimal precioUnitarioConImpuesto,
            decimal descuento,
            decimal impuestoPorcentaje)
        {
            if (cantidad <= 0)
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor a cero.");
            if (precioUnitarioConImpuesto < 0)
                throw new ArgumentOutOfRangeException(nameof(precioUnitarioConImpuesto), "El precio no puede ser negativo.");
            if (descuento < 0)
                throw new ArgumentOutOfRangeException(nameof(descuento), "El descuento no puede ser negativo.");

            var totalLinea = Redondear(cantidad * precioUnitarioConImpuesto - descuento);
            if (totalLinea < 0)
                throw new ArgumentOutOfRangeException(nameof(descuento), "El descuento supera el importe de la línea.");

            return Desglosar(totalLinea, impuestoPorcentaje);
        }

        public static IReadOnlyList<PrecioDesglosado> DesglosarConDescuentoGlobal(
            IReadOnlyList<(decimal Total, decimal ImpuestoPorcentaje)> lineas,
            decimal descuentoGlobal)
        {
            if (lineas is null)
                throw new ArgumentNullException(nameof(lineas));
            if (descuentoGlobal < 0)
                throw new ArgumentOutOfRangeException(nameof(descuentoGlobal), "El descuento global no puede ser negativo.");

            var totalesRedondeados = lineas.Select(l => Redondear(l.Total)).ToArray();
            var totalGeneral = Redondear(totalesRedondeados.Sum());

            if (descuentoGlobal > totalGeneral)
                throw new ArgumentOutOfRangeException(nameof(descuentoGlobal), "El descuento global supera el total.");

            if (lineas.Count == 0)
                return Array.Empty<PrecioDesglosado>();

            if (descuentoGlobal == 0 || totalGeneral == 0)
            {
                return lineas
                    .Select(l => Desglosar(l.Total, l.ImpuestoPorcentaje))
                    .ToArray();
            }

            var resultado = new PrecioDesglosado[lineas.Count];
            var descuentoAsignado = 0m;

            for (var i = 0; i < lineas.Count; i++)
            {
                var esUltima = i == lineas.Count - 1;
                var descuentoLinea = esUltima
                    ? descuentoGlobal - descuentoAsignado
                    : Redondear(descuentoGlobal * totalesRedondeados[i] / totalGeneral);

                if (descuentoLinea < 0)
                    descuentoLinea = 0m;
                if (descuentoLinea > totalesRedondeados[i])
                    descuentoLinea = totalesRedondeados[i];

                descuentoAsignado += descuentoLinea;
                var totalLinea = Redondear(totalesRedondeados[i] - descuentoLinea);
                resultado[i] = Desglosar(totalLinea, lineas[i].ImpuestoPorcentaje);
            }

            return resultado;
        }
    }
}

using Mercurio.Domain.Calculations;

namespace Mercurio.Tests;

public class PricingCalculatorTests
{
    [Theory]
    [InlineData(113, 13, 100, 13)]
    [InlineData(100, 13, 88.50, 11.50)]
    [InlineData(13, 13, 11.50, 1.50)]
    [InlineData(0, 13, 0, 0)]
    public void Desglosar_ConIvaIncluido_DevuelveBaseEImpuesto(
        decimal total,
        decimal tasa,
        decimal baseEsperada,
        decimal impuestoEsperado)
    {
        var resultado = PricingCalculator.Desglosar(total, tasa);

        Assert.Equal(baseEsperada, resultado.BaseImponible);
        Assert.Equal(impuestoEsperado, resultado.Impuesto);
        Assert.Equal(total, resultado.Total);
        Assert.Equal(total, resultado.BaseImponible + resultado.Impuesto);
    }

    [Fact]
    public void DesglosarLinea_AplicaDescuentoAntesDeSepararIva()
    {
        var resultado = PricingCalculator.DesglosarLinea(
            cantidad: 3,
            precioUnitarioConImpuesto: 113m,
            descuento: 13m,
            impuestoPorcentaje: 13m);

        Assert.Equal(326m, resultado.Total);
        Assert.Equal(288.50m, resultado.BaseImponible);
        Assert.Equal(37.50m, resultado.Impuesto);
    }

    [Fact]
    public void DesglosarLinea_RechazaDescuentoMayorAlImporte()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PricingCalculator.DesglosarLinea(1, 100m, 101m, 13m));
    }

    [Fact]
    public void DesglosarConDescuentoGlobal_DistribuyeProporcionalmenteYMantieneTotal()
    {
        var lineas = new List<(decimal Total, decimal ImpuestoPorcentaje)>
        {
            (113m, 13m),
            (226m, 13m)
        };

        var resultado = PricingCalculator.DesglosarConDescuentoGlobal(lineas, 39m);

        Assert.Equal(100m, resultado[0].Total);
        Assert.Equal(200m, resultado[1].Total);
        Assert.Equal(300m, resultado.Sum(r => r.Total));
        Assert.Equal(300m, resultado.Sum(r => r.BaseImponible + r.Impuesto));
        Assert.Equal(265.49m, resultado.Sum(r => r.BaseImponible));
        Assert.Equal(34.51m, resultado.Sum(r => r.Impuesto));
    }

    [Fact]
    public void DesglosarConDescuentoGlobal_ConRedondeo_ConservaElTotal()
    {
        var lineas = new List<(decimal Total, decimal ImpuestoPorcentaje)>
        {
            (10m, 13m),
            (10m, 13m),
            (10m, 13m)
        };

        var resultado = PricingCalculator.DesglosarConDescuentoGlobal(lineas, 1m);

        Assert.Equal(29m, resultado.Sum(r => r.Total));
        Assert.Equal(29m, resultado.Sum(r => r.BaseImponible + r.Impuesto));
    }

    [Fact]
    public void DesglosarConDescuentoGlobal_RechazaDescuentoMayorAlTotal()
    {
        var lineas = new List<(decimal Total, decimal ImpuestoPorcentaje)> { (100m, 13m) };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PricingCalculator.DesglosarConDescuentoGlobal(lineas, 100.01m));
    }

    [Fact]
    public void Desglosar_RechazaTotalNegativo()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PricingCalculator.Desglosar(-1m, 13m));
    }
}

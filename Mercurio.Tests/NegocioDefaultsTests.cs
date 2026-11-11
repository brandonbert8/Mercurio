using Mercurio.Domain.Entities;

namespace Mercurio.Tests;

public class NegocioDefaultsTests
{
    [Fact]
    public void NuevoNegocio_UsaParametrosFiscalesDeBolivia()
    {
        var negocio = new Negocio();

        Assert.Equal("BOB", negocio.Moneda);
        Assert.Equal("America/La_Paz", negocio.ZonaHoraria);
        Assert.Equal(13m, negocio.ImpuestoPorcentaje);
        Assert.True(negocio.PrecioIncluyeImpuesto);
    }
}

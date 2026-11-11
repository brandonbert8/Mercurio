using Mercurio.Domain.Calculations;

namespace Mercurio.Application.Common
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message)
        {
        }
    }

    public static class Money
    {
        public static decimal Round(decimal value)
        {
            return PricingCalculator.Redondear(value);
        }
    }
}

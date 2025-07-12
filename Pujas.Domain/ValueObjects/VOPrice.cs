
using Pujas.Domain.Exceptions;

namespace Pujas.Domain.ValueObjects
{
    public class VOPrice
    {
        public decimal Value { get; private set; }

        public VOPrice(decimal value)
        {
            if (value <= 0)
                throw new InvalidPriceException();

            Value = value;
        }

        public decimal ToDecimal() => Value;
    }
}

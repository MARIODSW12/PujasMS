using Pujas.Domain.Exceptions;

namespace Pujas.Domain.ValueObjects
{
    public class VOAuctionId
    {
        public string Value { get; private set; }

        public VOAuctionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidAuctionIdException();

            if (!Guid.TryParse(value, out _))
                throw new InvalidAuctionIdException();

            Value = value;
        }

        public override string ToString() => Value;
    }
}

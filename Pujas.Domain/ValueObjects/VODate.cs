using Pujas.Domain.Exceptions;

namespace Pujas.Domain.ValueObjects
{
    public class VODate
    {
        public DateTime Value { get; private set; }

        public VODate(DateTime value)
        {
            if (value.CompareTo(DateTime.UtcNow) > 0)
                throw new InvalidDateException();

            Value = value;
        }

        public string ToString() => Value.ToString();
    }
}

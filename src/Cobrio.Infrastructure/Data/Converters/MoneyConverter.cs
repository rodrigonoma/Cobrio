using Cobrio.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cobrio.Infrastructure.Data.Converters;

public class MoneyConverter : ValueConverter<Money, long>
{
    public MoneyConverter()
        : base(
            money => money.Centavos,
            centavos => new Money(centavos, "BRL"))
    {
    }
}

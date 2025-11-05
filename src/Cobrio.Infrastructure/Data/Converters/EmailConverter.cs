using Cobrio.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cobrio.Infrastructure.Data.Converters;

public class EmailConverter : ValueConverter<Email, string>
{
    public EmailConverter()
        : base(
            email => email.Endereco,
            endereco => new Email(endereco))
    {
    }
}

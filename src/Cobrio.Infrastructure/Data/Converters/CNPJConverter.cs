using Cobrio.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Cobrio.Infrastructure.Data.Converters;

public class CNPJConverter : ValueConverter<CNPJ, string>
{
    public CNPJConverter()
        : base(
            cnpj => cnpj.Numero,
            numero => new CNPJ(numero))
    {
    }
}

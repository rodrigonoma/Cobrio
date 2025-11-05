namespace Cobrio.Domain.ValueObjects;

public class Money : IEquatable<Money>
{
    public long Centavos { get; private set; }
    public string Moeda { get; private set; }

    public Money(long centavos, string moeda = "BRL")
    {
        if (centavos < 0)
            throw new ArgumentException("Valor não pode ser negativo", nameof(centavos));

        if (string.IsNullOrWhiteSpace(moeda) || moeda.Length != 3)
            throw new ArgumentException("Moeda deve ter 3 caracteres (ex: BRL)", nameof(moeda));

        Centavos = centavos;
        Moeda = moeda.ToUpper();
    }

    public decimal Valor => Centavos / 100.0m;

    public static Money FromDecimal(decimal valor, string moeda = "BRL")
    {
        return new Money((long)(valor * 100), moeda);
    }

    public static Money Zero(string moeda = "BRL") => new Money(0, moeda);

    public Money Add(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível somar valores de moedas diferentes");

        return new Money(Centavos + other.Centavos, Moeda);
    }

    public Money Subtract(Money other)
    {
        if (Moeda != other.Moeda)
            throw new InvalidOperationException("Não é possível subtrair valores de moedas diferentes");

        return new Money(Centavos - other.Centavos, Moeda);
    }

    public Money Multiply(decimal multiplicador)
    {
        return new Money((long)(Centavos * multiplicador), Moeda);
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Centavos == other.Centavos && Moeda == other.Moeda;
    }

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Centavos, Moeda);

    public override string ToString() => $"{Moeda} {Valor:N2}";

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    public static bool operator >(Money left, Money right)
    {
        if (left.Moeda != right.Moeda)
            throw new InvalidOperationException("Não é possível comparar valores de moedas diferentes");
        return left.Centavos > right.Centavos;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Moeda != right.Moeda)
            throw new InvalidOperationException("Não é possível comparar valores de moedas diferentes");
        return left.Centavos < right.Centavos;
    }
}

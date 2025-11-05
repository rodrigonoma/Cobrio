using System.Text.RegularExpressions;

namespace Cobrio.Domain.ValueObjects;

public class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Endereco { get; private set; }

    public Email(string endereco)
    {
        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("E-mail não pode ser vazio", nameof(endereco));

        endereco = endereco.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(endereco))
            throw new ArgumentException("E-mail inválido", nameof(endereco));

        if (endereco.Length > 100)
            throw new ArgumentException("E-mail não pode ter mais de 100 caracteres", nameof(endereco));

        Endereco = endereco;
    }

    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Endereco == other.Endereco;
    }

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Endereco.GetHashCode();

    public override string ToString() => Endereco;

    public static implicit operator string(Email email) => email.Endereco;

    public static explicit operator Email(string endereco) => new(endereco);
}

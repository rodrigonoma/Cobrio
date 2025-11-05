using System.Text.RegularExpressions;

namespace Cobrio.Domain.ValueObjects;

public class CNPJ : IEquatable<CNPJ>
{
    private static readonly Regex CnpjRegex = new(@"^\d{14}$", RegexOptions.Compiled);

    public string Numero { get; private set; }

    public CNPJ(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("CNPJ não pode ser vazio", nameof(numero));

        // Remove formatação
        numero = Regex.Replace(numero, @"[^\d]", "");

        if (!CnpjRegex.IsMatch(numero))
            throw new ArgumentException("CNPJ deve conter 14 dígitos", nameof(numero));

        if (!ValidarCNPJ(numero))
            throw new ArgumentException("CNPJ inválido", nameof(numero));

        Numero = numero;
    }

    private static bool ValidarCNPJ(string cnpj)
    {
        // Validação básica de CNPJ
        int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCnpj = cnpj.Substring(0, 12);
        int soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        string digito = resto.ToString();
        tempCnpj += digito;
        soma = 0;

        for (int i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cnpj.EndsWith(digito);
    }

    public string NumeroFormatado =>
        $"{Numero.Substring(0, 2)}.{Numero.Substring(2, 3)}.{Numero.Substring(5, 3)}/{Numero.Substring(8, 4)}-{Numero.Substring(12, 2)}";

    public bool Equals(CNPJ? other)
    {
        if (other is null) return false;
        return Numero == other.Numero;
    }

    public override bool Equals(object? obj) => Equals(obj as CNPJ);

    public override int GetHashCode() => Numero.GetHashCode();

    public override string ToString() => NumeroFormatado;

    public static implicit operator string(CNPJ cnpj) => cnpj.Numero;

    public static explicit operator CNPJ(string numero) => new(numero);
}

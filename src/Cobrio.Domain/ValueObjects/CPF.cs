using System.Text.RegularExpressions;

namespace Cobrio.Domain.ValueObjects;

public class CPF : IEquatable<CPF>
{
    private static readonly Regex CpfRegex = new(@"^\d{11}$", RegexOptions.Compiled);

    public string Numero { get; private set; }

    public CPF(string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("CPF não pode ser vazio", nameof(numero));

        // Remove formatação
        numero = Regex.Replace(numero, @"[^\d]", "");

        if (!CpfRegex.IsMatch(numero))
            throw new ArgumentException("CPF deve conter 11 dígitos", nameof(numero));

        if (!ValidarCPF(numero))
            throw new ArgumentException("CPF inválido", nameof(numero));

        Numero = numero;
    }

    private static bool ValidarCPF(string cpf)
    {
        // CPFs conhecidos como inválidos
        if (cpf == "00000000000" || cpf == "11111111111" || cpf == "22222222222" ||
            cpf == "33333333333" || cpf == "44444444444" || cpf == "55555555555" ||
            cpf == "66666666666" || cpf == "77777777777" || cpf == "88888888888" ||
            cpf == "99999999999")
            return false;

        int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        string tempCpf = cpf.Substring(0, 9);
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        string digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    public string NumeroFormatado =>
        $"{Numero.Substring(0, 3)}.{Numero.Substring(3, 3)}.{Numero.Substring(6, 3)}-{Numero.Substring(9, 2)}";

    public bool Equals(CPF? other)
    {
        if (other is null) return false;
        return Numero == other.Numero;
    }

    public override bool Equals(object? obj) => Equals(obj as CPF);

    public override int GetHashCode() => Numero.GetHashCode();

    public override string ToString() => NumeroFormatado;

    public static implicit operator string(CPF cpf) => cpf.Numero;

    public static explicit operator CPF(string numero) => new(numero);
}

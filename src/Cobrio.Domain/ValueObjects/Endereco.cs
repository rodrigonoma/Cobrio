namespace Cobrio.Domain.ValueObjects;

public class Endereco : IEquatable<Endereco>
{
    public string Logradouro { get; private set; } = null!;
    public string Numero { get; private set; } = null!;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public string CEP { get; private set; } = null!;
    public string Pais { get; private set; } = null!;

    // Construtor privado para EF Core
    private Endereco() { }

    public Endereco(
        string logradouro,
        string numero,
        string bairro,
        string cidade,
        string estado,
        string cep,
        string? complemento = null,
        string pais = "Brasil")
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new ArgumentException("Logradouro não pode ser vazio", nameof(logradouro));

        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Número não pode ser vazio", nameof(numero));

        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro não pode ser vazio", nameof(bairro));

        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia", nameof(cidade));

        if (string.IsNullOrWhiteSpace(estado) || estado.Length != 2)
            throw new ArgumentException("Estado deve ter 2 caracteres (ex: SP)", nameof(estado));

        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP não pode ser vazio", nameof(cep));

        // Remove formatação do CEP
        cep = cep.Replace("-", "").Replace(".", "").Trim();

        if (cep.Length != 8 || !cep.All(char.IsDigit))
            throw new ArgumentException("CEP deve conter 8 dígitos", nameof(cep));

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = complemento?.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpper();
        CEP = cep;
        Pais = pais.Trim();
    }

    public string CEPFormatado => $"{CEP.Substring(0, 5)}-{CEP.Substring(5, 3)}";

    public string EnderecoCompleto
    {
        get
        {
            var complementoStr = string.IsNullOrWhiteSpace(Complemento) ? "" : $", {Complemento}";
            return $"{Logradouro}, {Numero}{complementoStr} - {Bairro}, {Cidade}/{Estado} - {CEPFormatado}";
        }
    }

    public bool Equals(Endereco? other)
    {
        if (other is null) return false;
        return CEP == other.CEP && Numero == other.Numero;
    }

    public override bool Equals(object? obj) => Equals(obj as Endereco);

    public override int GetHashCode() => HashCode.Combine(CEP, Numero);

    public override string ToString() => EnderecoCompleto;
}

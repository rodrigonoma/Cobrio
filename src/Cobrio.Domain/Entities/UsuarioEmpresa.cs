using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class UsuarioEmpresa : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }

    public string Nome { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    public PerfilUsuario Perfil { get; private set; }
    public bool Ativo { get; private set; }
    public bool EhProprietario { get; private set; }

    public DateTime? UltimoAcesso { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private UsuarioEmpresa() { }

    public UsuarioEmpresa(
        Guid empresaClienteId,
        string nome,
        Email email,
        string passwordHash,
        PerfilUsuario perfil = PerfilUsuario.Operador,
        bool ehProprietario = false)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("PasswordHash não pode ser vazio", nameof(passwordHash));

        EmpresaClienteId = empresaClienteId;
        Nome = nome.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash;
        Perfil = perfil;
        Ativo = true;
        EhProprietario = ehProprietario;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        Nome = nome.Trim();
        AtualizarDataModificacao();
    }

    public void AtualizarEmail(Email email)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        AtualizarDataModificacao();
    }

    public void AtualizarSenha(string novoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(novoPasswordHash))
            throw new ArgumentException("PasswordHash não pode ser vazio", nameof(novoPasswordHash));

        PasswordHash = novoPasswordHash;
        AtualizarDataModificacao();
    }

    public void AlterarPerfil(PerfilUsuario novoPerfil)
    {
        if (EhProprietario && novoPerfil != PerfilUsuario.Admin)
            throw new InvalidOperationException("O proprietário deve manter o perfil de Administrador");

        Perfil = novoPerfil;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        if (EhProprietario)
            throw new InvalidOperationException("O usuário proprietário não pode ser desativado");

        Ativo = false;
        AtualizarDataModificacao();
    }

    public void MarcarComoProprietario()
    {
        EhProprietario = true;
        Perfil = PerfilUsuario.Admin; // Proprietário sempre é Admin
        AtualizarDataModificacao();
    }

    public void RegistrarAcesso()
    {
        UltimoAcesso = DateTime.UtcNow;
        AtualizarDataModificacao();
    }

    public bool PodeAcessarRecurso(PerfilUsuario perfilMinimo)
    {
        return Ativo && Perfil <= perfilMinimo;
    }
}

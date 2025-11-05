using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

/// <summary>
/// Representa a permissão de um perfil para executar uma ação em um módulo
/// </summary>
public class PermissaoPerfil : BaseEntity
{
    // Construtor para EF Core
    private PermissaoPerfil() { }

    public PermissaoPerfil(
        Guid empresaClienteId,
        PerfilUsuario perfilUsuario,
        Guid moduloId,
        Guid acaoId,
        bool permitido,
        Guid criadoPorUsuarioId)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (moduloId == Guid.Empty)
            throw new ArgumentException("ModuloId inválido", nameof(moduloId));

        if (acaoId == Guid.Empty)
            throw new ArgumentException("AcaoId inválido", nameof(acaoId));

        EmpresaClienteId = empresaClienteId;
        PerfilUsuario = perfilUsuario;
        ModuloId = moduloId;
        AcaoId = acaoId;
        Permitido = permitido;
        CriadoPorUsuarioId = criadoPorUsuarioId;
    }

    public Guid EmpresaClienteId { get; private set; } // Multi-tenant
    public PerfilUsuario PerfilUsuario { get; private set; }
    public Guid ModuloId { get; private set; }
    public Guid AcaoId { get; private set; }
    public bool Permitido { get; private set; }
    public Guid CriadoPorUsuarioId { get; private set; }

    // Relacionamentos
    public EmpresaCliente? EmpresaCliente { get; private set; }
    public Modulo? Modulo { get; private set; }
    public Acao? Acao { get; private set; }
    public UsuarioEmpresa? CriadoPor { get; private set; }

    public void Permitir()
    {
        Permitido = true;
        AtualizarDataModificacao();
    }

    public void Negar()
    {
        Permitido = false;
        AtualizarDataModificacao();
    }

    public void Alternar()
    {
        Permitido = !Permitido;
        AtualizarDataModificacao();
    }
}

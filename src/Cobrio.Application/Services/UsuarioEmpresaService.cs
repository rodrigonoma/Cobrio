using Cobrio.Application.DTOs.UsuarioEmpresa;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Application.Services;

public class UsuarioEmpresaService
{
    private readonly IUsuarioEmpresaRepository _usuarioRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UsuarioEmpresaService(
        IUsuarioEmpresaRepository usuarioRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _usuarioRepository = usuarioRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<UsuarioEmpresaResponse>> GetAllByEmpresaAsync(
        Guid empresaClienteId,
        CancellationToken cancellationToken = default)
    {
        var usuarios = await _usuarioRepository.GetByEmpresaIdAsync(empresaClienteId, cancellationToken);
        return usuarios.Select(MapToResponse);
    }

    public async Task<UsuarioEmpresaResponse> GetByIdAsync(
        Guid empresaClienteId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAndEmpresaAsync(id, empresaClienteId, cancellationToken);

        if (usuario == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");

        return MapToResponse(usuario);
    }

    public async Task<UsuarioEmpresaResponse> CreateAsync(
        Guid empresaClienteId,
        CreateUsuarioEmpresaRequest request,
        CancellationToken cancellationToken = default)
    {
        // Verificar se email já existe na empresa
        if (await _usuarioRepository.EmailExistsAsync(request.Email, empresaClienteId, cancellationToken))
            throw new InvalidOperationException($"Email '{request.Email}' já está em uso nesta empresa");

        // Hash da senha usando BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        // Criar email Value Object
        var email = new Email(request.Email);

        // Criar usuário
        var usuario = new UsuarioEmpresa(
            empresaClienteId,
            request.Nome,
            email,
            passwordHash,
            request.Perfil
        );

        // Auditoria: Definir usuário de criação
        if (_currentUserService.UserId.HasValue)
        {
            usuario.DefinirUsuarioCriacao(_currentUserService.UserId.Value);
        }

        await _usuarioRepository.AddAsync(usuario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(usuario);
    }

    public async Task<UsuarioEmpresaResponse> UpdateAsync(
        Guid empresaClienteId,
        Guid id,
        UpdateUsuarioEmpresaRequest request,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAndEmpresaAsync(id, empresaClienteId, cancellationToken);

        if (usuario == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");

        // Obter usuário logado
        var currentUser = await _usuarioRepository.GetByIdAndEmpresaAsync(currentUserId, empresaClienteId, cancellationToken);

        if (currentUser == null)
            throw new UnauthorizedAccessException("Usuário logado não encontrado");

        // Proprietário não pode ser editado
        if (usuario.EhProprietario)
            throw new InvalidOperationException("O usuário proprietário não pode ser editado");

        // Admin não pode editar outro Admin (apenas Proprietário pode)
        if (usuario.Perfil == PerfilUsuario.Admin && !currentUser.EhProprietario)
            throw new InvalidOperationException("Apenas o proprietário pode editar usuários Admin");

        // Atualizar dados
        usuario.AtualizarNome(request.Nome);
        usuario.AlterarPerfil(request.Perfil);

        if (request.Ativo && !usuario.Ativo)
            usuario.Ativar();
        else if (!request.Ativo && usuario.Ativo)
            usuario.Desativar();

        // Auditoria: Registrar usuário de modificação
        if (_currentUserService.UserId.HasValue)
        {
            usuario.AtualizarDataModificacao(_currentUserService.UserId.Value);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(usuario);
    }

    public async Task DeleteAsync(
        Guid empresaClienteId,
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAndEmpresaAsync(id, empresaClienteId, cancellationToken);

        if (usuario == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");

        // Obter usuário logado
        var currentUser = await _usuarioRepository.GetByIdAndEmpresaAsync(currentUserId, empresaClienteId, cancellationToken);

        if (currentUser == null)
            throw new UnauthorizedAccessException("Usuário logado não encontrado");

        // Proprietário não pode ser excluído
        if (usuario.EhProprietario)
            throw new InvalidOperationException("O usuário proprietário não pode ser excluído");

        // Admin não pode excluir outro Admin (apenas Proprietário pode)
        if (usuario.Perfil == PerfilUsuario.Admin && !currentUser.EhProprietario)
            throw new InvalidOperationException("Apenas o proprietário pode excluir usuários Admin");

        // Não permitir deletar, apenas desativar
        usuario.Desativar();
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task ResetarSenhaAsync(
        Guid empresaClienteId,
        Guid id,
        ResetarSenhaRequest request,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.GetByIdAndEmpresaAsync(id, empresaClienteId, cancellationToken);

        if (usuario == null)
            throw new KeyNotFoundException($"Usuário com ID {id} não encontrado");

        // Hash da nova senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        // Atualizar senha
        usuario.AtualizarSenha(passwordHash);

        // Auditoria: Registrar usuário de modificação
        if (_currentUserService.UserId.HasValue)
        {
            usuario.AtualizarDataModificacao(_currentUserService.UserId.Value);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private UsuarioEmpresaResponse MapToResponse(UsuarioEmpresa usuario)
    {
        return new UsuarioEmpresaResponse
        {
            Id = usuario.Id,
            EmpresaClienteId = usuario.EmpresaClienteId,
            Nome = usuario.Nome,
            Email = usuario.Email.Endereco,
            Perfil = usuario.Perfil,
            PerfilDescricao = ObterDescricaoPerfil(usuario.Perfil),
            Ativo = usuario.Ativo,
            EhProprietario = usuario.EhProprietario,
            UltimoAcesso = usuario.UltimoAcesso,
            CriadoEm = usuario.CriadoEm,
            AtualizadoEm = usuario.AtualizadoEm
        };
    }

    private string ObterDescricaoPerfil(PerfilUsuario perfil)
    {
        return perfil switch
        {
            PerfilUsuario.Admin => "Administrador",
            PerfilUsuario.Operador => "Operador",
            _ => perfil.ToString()
        };
    }
}

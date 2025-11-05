using Cobrio.Application.DTOs.Auth;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cobrio.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly int _refreshTokenExpirationDays;

    public AuthService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenExpirationDays = int.Parse(configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        // Buscar usuário pelo email
        var usuario = await _unitOfWork.Usuarios.GetByEmailWithEmpresaAsync(request.Email);

        if (usuario == null)
        {
            // Log detalhado para debug (não expor ao cliente)
            _logger.LogWarning("Tentativa de login com email não cadastrado: {Email}", request.Email);
            // Mensagem genérica para não revelar que email não existe (segurança)
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Verificar senha (usando BCrypt)
        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Tentativa de login com senha incorreta para usuário: {Email}", request.Email);
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Verificar se usuário está ativo
        if (!usuario.Ativo)
        {
            _logger.LogWarning("Tentativa de login com usuário inativo: {Email}", request.Email);
            throw new UnauthorizedAccessException("Usuário inativo. Entre em contato com o administrador.");
        }

        // Gerar tokens
        var accessToken = _tokenService.GenerateAccessToken(usuario);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Salvar refresh token no banco
        var refreshTokenEntity = new RefreshToken(
            usuario.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            ipAddress
        );

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);

        // Atualizar último acesso
        usuario.RegistrarAcesso();
        await _unitOfWork.CommitAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = new UserInfo
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email.Endereco,
                Perfil = usuario.Perfil.ToString(),
                EhProprietario = usuario.EhProprietario,
                Ativo = usuario.Ativo,
                EmpresaClienteId = usuario.EmpresaClienteId,
                EmpresaClienteNome = usuario.EmpresaCliente.Nome
            }
        };
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        // Buscar refresh token no banco
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (token == null)
            throw new UnauthorizedAccessException("Refresh token inválido");

        // Validar se token está ativo
        if (!token.IsActive)
            throw new UnauthorizedAccessException("Refresh token expirado ou revogado");

        var usuario = token.UsuarioEmpresa;

        // Verificar se usuário está ativo
        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo");

        // Revogar token antigo
        token.Revoke(ipAddress);

        // Gerar novos tokens
        var accessToken = _tokenService.GenerateAccessToken(usuario);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Salvar novo refresh token
        var newRefreshTokenEntity = new RefreshToken(
            usuario.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            ipAddress
        );

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
        await _unitOfWork.CommitAsync();

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = new UserInfo
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email.Endereco,
                Perfil = usuario.Perfil.ToString(),
                EhProprietario = usuario.EhProprietario,
                Ativo = usuario.Ativo,
                EmpresaClienteId = usuario.EmpresaClienteId,
                EmpresaClienteNome = usuario.EmpresaCliente.Nome
            }
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, string ipAddress)
    {
        var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (token == null)
            throw new InvalidOperationException("Token não encontrado");

        if (!token.IsActive)
            throw new InvalidOperationException("Token já está inativo");

        token.Revoke(ipAddress);
        await _unitOfWork.CommitAsync();
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var usuario = await _unitOfWork.Usuarios.GetByEmailAsync(email);

        if (usuario == null || !usuario.Ativo)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
    }
}

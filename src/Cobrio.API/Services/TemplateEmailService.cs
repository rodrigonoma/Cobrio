using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Cobrio.API.Services;

public class TemplateEmailService
{
    private readonly ITemplateEmailRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TemplateEmailService(
        ITemplateEmailRepository templateRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetCurrentEmpresaId()
    {
        var empresaId = _httpContextAccessor.HttpContext?.Items["TenantId"] as Guid?;
        if (empresaId == null || empresaId == Guid.Empty)
            throw new UnauthorizedAccessException("EmpresaClienteId não encontrado no contexto");

        return empresaId.Value;
    }

    public async Task<IEnumerable<TemplateEmailDto>> GetAllAsync()
    {
        var empresaId = GetCurrentEmpresaId();
        var templates = await _templateRepository.GetByEmpresaIdAsync(empresaId);

        return templates.Select(t => new TemplateEmailDto
        {
            Id = t.Id,
            Nome = t.Nome,
            Descricao = t.Descricao,
            ConteudoHtml = t.ConteudoHtml,
            SubjectEmail = t.SubjectEmail,
            VariaveisObrigatorias = t.GetVariaveisObrigatorias(),
            VariaveisObrigatoriasSistema = t.GetVariaveisObrigatoriasSistema(),
            CanalSugerido = t.CanalSugerido,
            CriadoEm = t.CriadoEm,
            AtualizadoEm = t.AtualizadoEm
        });
    }

    public async Task<TemplateEmailDto?> GetByIdAsync(Guid id)
    {
        var empresaId = GetCurrentEmpresaId();
        var template = await _templateRepository.GetByIdAsync(id);

        if (template == null || template.EmpresaClienteId != empresaId)
            return null;

        return new TemplateEmailDto
        {
            Id = template.Id,
            Nome = template.Nome,
            Descricao = template.Descricao,
            ConteudoHtml = template.ConteudoHtml,
            SubjectEmail = template.SubjectEmail,
            VariaveisObrigatorias = template.GetVariaveisObrigatorias(),
            VariaveisObrigatoriasSistema = template.GetVariaveisObrigatoriasSistema(),
            CanalSugerido = template.CanalSugerido,
            CriadoEm = template.CriadoEm,
            AtualizadoEm = template.AtualizadoEm
        };
    }

    public async Task<TemplateEmailDto> CreateAsync(CreateTemplateEmailDto dto)
    {
        var empresaId = GetCurrentEmpresaId();

        // Verificar se já existe um template com o mesmo nome
        var existente = await _templateRepository.GetByNomeAsync(empresaId, dto.Nome);
        if (existente != null)
            throw new InvalidOperationException($"Já existe um template com o nome '{dto.Nome}'");

        var template = new TemplateEmail(
            empresaId,
            dto.Nome,
            dto.ConteudoHtml,
            dto.Descricao,
            dto.SubjectEmail,
            dto.VariaveisObrigatoriasSistema,
            dto.CanalSugerido
        );

        await _templateRepository.AddAsync(template);
        await _unitOfWork.CommitAsync();

        return new TemplateEmailDto
        {
            Id = template.Id,
            Nome = template.Nome,
            Descricao = template.Descricao,
            ConteudoHtml = template.ConteudoHtml,
            SubjectEmail = template.SubjectEmail,
            VariaveisObrigatorias = template.GetVariaveisObrigatorias(),
            VariaveisObrigatoriasSistema = template.GetVariaveisObrigatoriasSistema(),
            CanalSugerido = template.CanalSugerido,
            CriadoEm = template.CriadoEm,
            AtualizadoEm = template.AtualizadoEm
        };
    }

    public async Task<TemplateEmailDto?> UpdateAsync(Guid id, UpdateTemplateEmailDto dto)
    {
        var empresaId = GetCurrentEmpresaId();
        var template = await _templateRepository.GetByIdAsync(id);

        if (template == null || template.EmpresaClienteId != empresaId)
            return null;

        // Se está alterando o nome, verificar se não existe outro com o mesmo nome
        if (!string.IsNullOrWhiteSpace(dto.Nome) && dto.Nome != template.Nome)
        {
            var existente = await _templateRepository.GetByNomeAsync(empresaId, dto.Nome);
            if (existente != null && existente.Id != id)
                throw new InvalidOperationException($"Já existe outro template com o nome '{dto.Nome}'");
        }

        template.Atualizar(
            dto.Nome,
            dto.Descricao,
            dto.ConteudoHtml,
            dto.SubjectEmail,
            dto.VariaveisObrigatoriasSistema,
            dto.CanalSugerido
        );

        _templateRepository.Update(template);
        await _unitOfWork.CommitAsync();

        return new TemplateEmailDto
        {
            Id = template.Id,
            Nome = template.Nome,
            Descricao = template.Descricao,
            ConteudoHtml = template.ConteudoHtml,
            SubjectEmail = template.SubjectEmail,
            VariaveisObrigatorias = template.GetVariaveisObrigatorias(),
            VariaveisObrigatoriasSistema = template.GetVariaveisObrigatoriasSistema(),
            CanalSugerido = template.CanalSugerido,
            CriadoEm = template.CriadoEm,
            AtualizadoEm = template.AtualizadoEm
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var empresaId = GetCurrentEmpresaId();
        var template = await _templateRepository.GetByIdAsync(id);

        if (template == null || template.EmpresaClienteId != empresaId)
            return false;

        _templateRepository.Remove(template);
        await _unitOfWork.CommitAsync();
        return true;
    }
}

// DTOs
public class TemplateEmailDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string ConteudoHtml { get; set; } = string.Empty;
    public string? SubjectEmail { get; set; }
    public List<string> VariaveisObrigatorias { get; set; } = new();
    public List<string> VariaveisObrigatoriasSistema { get; set; } = new();
    public CanalNotificacao? CanalSugerido { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }
}

public class CreateTemplateEmailDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string ConteudoHtml { get; set; } = string.Empty;
    public string? SubjectEmail { get; set; }
    public List<string>? VariaveisObrigatoriasSistema { get; set; }
    public CanalNotificacao? CanalSugerido { get; set; }
}

public class UpdateTemplateEmailDto
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public string? ConteudoHtml { get; set; }
    public string? SubjectEmail { get; set; }
    public List<string>? VariaveisObrigatoriasSistema { get; set; }
    public CanalNotificacao? CanalSugerido { get; set; }
}

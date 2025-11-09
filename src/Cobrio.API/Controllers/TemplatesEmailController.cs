using Cobrio.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TemplatesEmailController : ControllerBase
{
    private readonly TemplateEmailService _templateService;
    private readonly ILogger<TemplatesEmailController> _logger;

    public TemplatesEmailController(
        TemplateEmailService templateService,
        ILogger<TemplatesEmailController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemplateEmailDto>>> GetAll()
    {
        try
        {
            var templates = await _templateService.GetAllAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar templates de email");
            return StatusCode(500, new { message = "Erro ao buscar templates" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateEmailDto>> GetById(Guid id)
    {
        try
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
                return NotFound(new { message = "Template não encontrado" });

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar template {Id}", id);
            return StatusCode(500, new { message = "Erro ao buscar template" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<TemplateEmailDto>> Create([FromBody] CreateTemplateEmailDto dto)
    {
        try
        {
            var template = await _templateService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = template.Id }, template);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar template");
            return StatusCode(500, new { message = "Erro ao criar template" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateEmailDto>> Update(Guid id, [FromBody] UpdateTemplateEmailDto dto)
    {
        try
        {
            var template = await _templateService.UpdateAsync(id, dto);
            if (template == null)
                return NotFound(new { message = "Template não encontrado" });

            return Ok(template);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar template {Id}", id);
            return StatusCode(500, new { message = "Erro ao atualizar template" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _templateService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Template não encontrado" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar template {Id}", id);
            return StatusCode(500, new { message = "Erro ao deletar template" });
        }
    }
}

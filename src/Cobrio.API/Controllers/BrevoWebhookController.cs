using Cobrio.Application.DTOs.Brevo;
using Cobrio.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/webhook/brevo")]
public class BrevoWebhookController : ControllerBase
{
    private readonly BrevoWebhookService _brevoWebhookService;
    private readonly ILogger<BrevoWebhookController> _logger;

    public BrevoWebhookController(
        BrevoWebhookService brevoWebhookService,
        ILogger<BrevoWebhookController> logger)
    {
        _brevoWebhookService = brevoWebhookService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint para receber webhooks do Brevo
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ReceberEvento([FromBody] BrevoWebhookEvent evento, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Webhook recebido do Brevo: {Event} para {Email}", evento.Event, evento.Email);

            var sucesso = await _brevoWebhookService.ProcessarEventoAsync(evento, cancellationToken);

            if (sucesso)
            {
                return Ok(new { message = "Evento processado com sucesso" });
            }
            else
            {
                return BadRequest(new { message = "Falha ao processar evento" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook do Brevo");
            return StatusCode(500, new { message = "Erro interno ao processar evento" });
        }
    }

    /// <summary>
    /// Endpoint de teste para validar se o webhook está funcionando
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", message = "Webhook Brevo está funcionando" });
    }
}

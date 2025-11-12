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

            // Capturar informações da requisição HTTP
            var enderecoIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            var headers = System.Text.Json.JsonSerializer.Serialize(
                HttpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            );

            var sucesso = await _brevoWebhookService.ProcessarEventoAsync(
                evento,
                enderecoIp,
                userAgent,
                headers,
                cancellationToken);

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

    /// <summary>
    /// Endpoint de teste para simular um webhook de abertura
    /// </summary>
    [HttpPost("teste-abertura")]
    public async Task<IActionResult> TesteAbertura([FromBody] TesteWebhookRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🧪 TESTE: Simulando webhook de abertura para MessageId: {MessageId}", request.MessageId);

            var evento = new BrevoWebhookEvent
            {
                Event = "unique_opened",
                Email = request.Email ?? "teste@exemplo.com",
                MessageId = request.MessageId,
                Id = request.Id ?? 0,
                TsEvent = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Ip = "127.0.0.1",
                UserAgent = "Teste Manual"
            };

            var sucesso = await _brevoWebhookService.ProcessarEventoAsync(
                evento,
                "127.0.0.1",
                "Teste Manual",
                null,
                cancellationToken);

            if (sucesso)
            {
                return Ok(new {
                    message = "Teste de abertura processado com sucesso",
                    messageId = request.MessageId
                });
            }
            else
            {
                return BadRequest(new {
                    message = "Falha ao processar teste de abertura. Verifique os logs.",
                    messageId = request.MessageId
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar teste de webhook");
            return StatusCode(500, new { message = "Erro no teste", error = ex.Message });
        }
    }

    /// <summary>
    /// Endpoint de debug para testar o payload EXATO do Brevo
    /// </summary>
    [HttpPost("debug-payload")]
    public async Task<IActionResult> DebugPayload([FromBody] BrevoWebhookEvent evento, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🐛 DEBUG: Recebido payload completo do Brevo");

            var sucesso = await _brevoWebhookService.ProcessarEventoAsync(
                evento,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.Request.Headers["User-Agent"].ToString(),
                null,
                cancellationToken);

            return Ok(new {
                message = sucesso ? "Processado com sucesso" : "Falhou ao processar",
                success = sucesso,
                evento = new {
                    evento.Event,
                    evento.Email,
                    evento.MessageId,
                    evento.Id,
                    evento.TsEvent
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar debug payload");
            return StatusCode(500, new { message = "Erro", error = ex.Message, stack = ex.StackTrace });
        }
    }
}

public class TesteWebhookRequest
{
    public string MessageId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public long? Id { get; set; }
}

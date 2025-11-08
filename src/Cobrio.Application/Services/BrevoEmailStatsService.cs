using Cobrio.Application.DTOs.Relatorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Cobrio.Application.Services;

public class BrevoEmailStatsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BrevoEmailStatsService> _logger;
    private readonly string _apiKey;

    public BrevoEmailStatsService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BrevoEmailStatsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["Brevo:ApiKey"] ?? throw new InvalidOperationException("Brevo API Key não configurada");

        _httpClient.BaseAddress = new Uri("https://api.brevo.com/v3/");
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<EstatisticasEmailResponse> GetEmailStatisticsAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        try
        {
            // Limitar a 90 dias (limite da API do Brevo)
            var diasDiferenca = (dataFim - dataInicio).Days;
            if (diasDiferenca > 90)
            {
                _logger.LogWarning("Período solicitado ({Dias} dias) excede o limite de 90 dias da API do Brevo. Ajustando para 90 dias.", diasDiferenca);
                dataInicio = dataFim.AddDays(-90);
            }

            // Brevo API usa formato ISO 8601 para datas
            var startDate = dataInicio.ToString("yyyy-MM-dd");
            var endDate = dataFim.ToString("yyyy-MM-dd");

            // Endpoint para obter estatísticas de emails transacionais
            var url = $"smtp/statistics/aggregatedReport?startDate={startDate}&endDate={endDate}";

            _logger.LogInformation("Buscando estatísticas do Brevo: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Resposta do Brevo - Status: {StatusCode}, Content: {Content}", response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erro ao buscar estatísticas do Brevo: {StatusCode} - {Content}", response.StatusCode, content);
                return new EstatisticasEmailResponse();
            }

            var brevoStats = JsonSerializer.Deserialize<BrevoStatsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (brevoStats == null)
            {
                _logger.LogWarning("Resposta do Brevo retornou null após deserialização");
                return new EstatisticasEmailResponse();
            }

            var totalEnviados = brevoStats.Requests;
            var totalAbertos = brevoStats.UniqueOpens;
            var totalClicados = brevoStats.UniqueClicks;
            var totalBounces = brevoStats.HardBounces + brevoStats.SoftBounces;

            _logger.LogInformation("Estatísticas obtidas do Brevo - Enviados: {Enviados}, Abertos: {Abertos}, Clicados: {Clicados}",
                totalEnviados, totalAbertos, totalClicados);

            return new EstatisticasEmailResponse
            {
                TotalEnviados = totalEnviados,
                TotalAbertos = totalAbertos,
                TotalClicados = totalClicados,
                TotalBounces = totalBounces,
                TaxaAbertura = totalEnviados > 0 ? Math.Round((decimal)totalAbertos / totalEnviados * 100, 2) : 0,
                TaxaClique = totalEnviados > 0 ? Math.Round((decimal)totalClicados / totalEnviados * 100, 2) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar estatísticas do Brevo");
            return new EstatisticasEmailResponse();
        }
    }

    // Modelo interno para deserialização da resposta do Brevo
    private class BrevoStatsResponse
    {
        public int Requests { get; set; }
        public int Delivered { get; set; }
        public int HardBounces { get; set; }
        public int SoftBounces { get; set; }
        public int Clicks { get; set; }
        public int UniqueClicks { get; set; }
        public int Opens { get; set; }
        public int UniqueOpens { get; set; }
        public int SpamReports { get; set; }
        public int Blocked { get; set; }
        public int Invalid { get; set; }
        public int Unsubscribed { get; set; }
    }
}

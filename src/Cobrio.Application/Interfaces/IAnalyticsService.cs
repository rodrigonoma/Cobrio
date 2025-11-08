using Cobrio.Application.DTOs.Analytics;

namespace Cobrio.Application.Interfaces;

public interface IAnalyticsService
{
    /// <summary>
    /// Obtém todos os dados analíticos do dashboard com filtros aplicados
    /// </summary>
    /// <param name="empresaClienteId">ID da empresa cliente (multi-tenant)</param>
    /// <param name="dias">Número de dias para análise (padrão: 30)</param>
    /// <param name="tipoRegua">Filtro de tipo de régua (null = todas)</param>
    /// <param name="canal">Filtro de canal (null = todos)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados completos de analytics do dashboard</returns>
    Task<DashboardAnalyticsResponse> GetDashboardAnalyticsAsync(
        Guid empresaClienteId,
        int dias = 30,
        string? tipoRegua = null,
        string? canal = null,
        CancellationToken cancellationToken = default);
}

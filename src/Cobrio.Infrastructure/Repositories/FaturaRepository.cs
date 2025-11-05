using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class FaturaRepository : Repository<Fatura>, IFaturaRepository
{
    public FaturaRepository(CobrioDbContext context) : base(context)
    {
    }

    public async Task<Fatura?> GetByNumeroFaturaAsync(
        string numeroFatura,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Itens)
            .Include(f => f.Tentativas)
            .FirstOrDefaultAsync(f => f.NumeroFatura == numeroFatura && f.EmpresaClienteId == empresaId,
                                cancellationToken);
    }

    public async Task<IEnumerable<Fatura>> GetPorAssinanteAsync(
        Guid assinanteId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.AssinanteId == assinanteId)
            .Include(f => f.Itens)
            .OrderByDescending(f => f.DataEmissao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fatura>> GetPorStatusAsync(
        Guid empresaId,
        StatusFatura status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.EmpresaClienteId == empresaId && f.Status == status)
            .Include(f => f.Assinante)
            .OrderByDescending(f => f.DataVencimento)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fatura>> GetVencidasAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var dataAtual = DateTime.UtcNow;

        return await _dbSet
            .Where(f => f.EmpresaClienteId == empresaId &&
                       f.DataVencimento < dataAtual &&
                       (f.Status == StatusFatura.Pendente || f.Status == StatusFatura.Falhou))
            .Include(f => f.Assinante)
            .Include(f => f.Tentativas)
            .OrderBy(f => f.DataVencimento)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Fatura>> GetFalhadasAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.EmpresaClienteId == empresaId && f.Status == StatusFatura.Falhou)
            .Include(f => f.Assinante)
            .Include(f => f.Tentativas)
            .OrderByDescending(f => f.AtualizadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> ObterReceitaMensalAsync(
        Guid empresaId,
        int mes,
        int ano,
        CancellationToken cancellationToken = default)
    {
        var faturasPagas = await _dbSet
            .Where(f => f.EmpresaClienteId == empresaId &&
                       f.Status == StatusFatura.Pago &&
                       f.DataPagamento != null &&
                       f.DataPagamento.Value.Year == ano &&
                       f.DataPagamento.Value.Month == mes)
            .ToListAsync(cancellationToken);

        return faturasPagas.Sum(f => f.ValorLiquido.Valor);
    }

    public async Task<string> GerarProximoNumeroFaturaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var anoAtual = DateTime.UtcNow.Year;
        var mesAtual = DateTime.UtcNow.Month;

        var ultimaFatura = await _dbSet
            .Where(f => f.EmpresaClienteId == empresaId &&
                       f.DataEmissao.Year == anoAtual &&
                       f.DataEmissao.Month == mesAtual)
            .OrderByDescending(f => f.NumeroFatura)
            .FirstOrDefaultAsync(cancellationToken);

        if (ultimaFatura == null)
        {
            return $"FAT-{anoAtual:0000}{mesAtual:00}-0001";
        }

        // Extrair o número sequencial
        var partes = ultimaFatura.NumeroFatura.Split('-');
        if (partes.Length >= 2 && int.TryParse(partes[^1], out var numero))
        {
            var novoNumero = numero + 1;
            return $"FAT-{anoAtual:0000}{mesAtual:00}-{novoNumero:0000}";
        }

        return $"FAT-{anoAtual:0000}{mesAtual:00}-0001";
    }
}

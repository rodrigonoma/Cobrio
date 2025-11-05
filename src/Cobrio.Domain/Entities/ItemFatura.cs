using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;

namespace Cobrio.Domain.Entities;

public class ItemFatura : BaseEntity
{
    public Guid FaturaId { get; private set; }

    public string Descricao { get; private set; }
    public int Quantidade { get; private set; }
    public Money ValorUnitario { get; private set; }
    public Money ValorTotal { get; private set; }

    public TipoItemFatura TipoItem { get; private set; }

    // Referência
    public Guid? PlanoOfertaId { get; private set; }

    // Navegação
    public Fatura Fatura { get; private set; } = null!;
    public PlanoOferta? PlanoOferta { get; private set; }

    // Construtor para EF Core
    private ItemFatura() { }

    public ItemFatura(
        Guid faturaId,
        string descricao,
        Money valorUnitario,
        int quantidade = 1,
        TipoItemFatura tipoItem = TipoItemFatura.Plano,
        Guid? planoOfertaId = null)
    {
        if (faturaId == Guid.Empty)
            throw new ArgumentException("FaturaId inválido", nameof(faturaId));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("Descrição não pode ser vazia", nameof(descricao));

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantidade));

        FaturaId = faturaId;
        Descricao = descricao.Trim();
        Quantidade = quantidade;
        ValorUnitario = valorUnitario ?? throw new ArgumentNullException(nameof(valorUnitario));
        ValorTotal = ValorUnitario.Multiply(quantidade);
        TipoItem = tipoItem;
        PlanoOfertaId = planoOfertaId;
    }
}

using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class Cobranca : BaseEntity
{
    public Guid RegraCobrancaId { get; private set; }
    public Guid EmpresaClienteId { get; private set; }

    // Payload JSON flexível enviado pelo cliente
    public string PayloadJson { get; private set; }

    // Data de vencimento extraída do payload ou calculada
    public DateTime DataVencimento { get; private set; }

    // Data em que deve ser disparada a notificação
    public DateTime DataDisparo { get; private set; }

    // Status da cobrança
    public StatusCobranca Status { get; private set; }

    // Controle de tentativas
    public int TentativasEnvio { get; private set; }
    public DateTime? DataProcessamento { get; private set; }
    public string? MensagemErro { get; private set; }

    // Navegação
    public RegraCobranca RegraCobranca { get; private set; } = null!;
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;
    public ICollection<HistoricoNotificacao> Historicos { get; private set; } = new List<HistoricoNotificacao>();

    // Construtor para EF Core
    private Cobranca() { }

    public Cobranca(
        Guid regraCobrancaId,
        Guid empresaClienteId,
        string payloadJson,
        DateTime dataVencimento,
        TipoMomento tipoMomento,
        int valorTempo,
        UnidadeTempo unidadeTempo,
        bool ehRegraPadrao = false)
    {
        if (regraCobrancaId == Guid.Empty)
            throw new ArgumentException("RegraCobrancaId inválido", nameof(regraCobrancaId));

        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("PayloadJson não pode ser vazio", nameof(payloadJson));

        // Se for regra padrão (Envio Imediato), dispara AGORA
        DateTime dataDisparo;
        if (ehRegraPadrao)
        {
            dataDisparo = DateTime.Now;
        }
        else
        {
            // Calcula quando o disparo deve acontecer
            dataDisparo = CalcularDataDisparo(dataVencimento, tipoMomento, valorTempo, unidadeTempo);

            // Valida se o momento do disparo está no passado
            // Usa DateTime.Now para comparar com a data recebida (que é em horário local)
            if (dataDisparo < DateTime.Now)
                throw new ArgumentException("O momento de disparo da notificação já passou. Ajuste a data de vencimento para uma data futura.", nameof(dataVencimento));
        }

        RegraCobrancaId = regraCobrancaId;
        EmpresaClienteId = empresaClienteId;
        PayloadJson = payloadJson;

        // Preserva a hora completa se a unidade for minutos ou horas, caso contrário usa apenas a data
        DataVencimento = (unidadeTempo == UnidadeTempo.Minutos || unidadeTempo == UnidadeTempo.Horas)
            ? dataVencimento
            : dataVencimento.Date;

        DataDisparo = dataDisparo;
        Status = StatusCobranca.Pendente;
        TentativasEnvio = 0;
    }

    private DateTime CalcularDataDisparo(DateTime dataVencimento, TipoMomento tipoMomento, int valorTempo, UnidadeTempo unidadeTempo)
    {
        // Se é exatamente no vencimento, retorna a data de vencimento
        if (tipoMomento == TipoMomento.Exatamente)
            return dataVencimento;

        // Calcula o offset baseado na unidade de tempo
        TimeSpan offset = unidadeTempo switch
        {
            UnidadeTempo.Minutos => TimeSpan.FromMinutes(valorTempo),
            UnidadeTempo.Horas => TimeSpan.FromHours(valorTempo),
            UnidadeTempo.Dias => TimeSpan.FromDays(valorTempo),
            _ => throw new ArgumentException("Unidade de tempo inválida", nameof(unidadeTempo))
        };

        // Aplica o offset baseado no tipo de momento
        return tipoMomento switch
        {
            TipoMomento.Antes => dataVencimento - offset,
            TipoMomento.Depois => dataVencimento + offset,
            _ => dataVencimento
        };
    }

    public void MarcarComoProcessada()
    {
        Status = StatusCobranca.Processada;
        DataProcessamento = DateTime.UtcNow;
        AtualizarDataModificacao();
    }

    public void MarcarComoFalha(string mensagemErro)
    {
        Status = StatusCobranca.Falha;
        MensagemErro = mensagemErro;
        TentativasEnvio++;
        AtualizarDataModificacao();
    }

    public void Cancelar()
    {
        Status = StatusCobranca.Cancelada;
        AtualizarDataModificacao();
    }

    public void ReprocessarAposFalha()
    {
        if (Status != StatusCobranca.Falha)
            throw new InvalidOperationException("Apenas cobranças com falha podem ser reprocessadas");

        Status = StatusCobranca.Pendente;
        MensagemErro = null;
        AtualizarDataModificacao();
    }

    public bool PodeSerProcessada()
    {
        return Status == StatusCobranca.Pendente &&
               DataDisparo <= DateTime.UtcNow &&
               TentativasEnvio < 5; // Máximo de 5 tentativas
    }

    public bool DeveTentarNovamente()
    {
        return Status == StatusCobranca.Falha && TentativasEnvio < 5;
    }
}

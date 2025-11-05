namespace Cobrio.Domain.Entities;

public class ReguaDunningConfig : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }

    // Tentativas
    public int NumeroMaximoTentativas { get; private set; }
    public List<int> IntervalosDias { get; private set; } = new(); // Ex: [1, 3, 7]

    // Métodos de contato
    public bool EnviarEmail { get; private set; }
    public bool EnviarSMS { get; private set; }
    public bool EnviarNotificacaoInApp { get; private set; }

    // Ações automáticas
    public int DiasSuspensao { get; private set; }
    public int DiasCancelamento { get; private set; }

    // Horários permitidos para retry
    public TimeSpan HoraInicioRetry { get; private set; }
    public TimeSpan HoraFimRetry { get; private set; }

    public bool Ativo { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private ReguaDunningConfig() { }

    public ReguaDunningConfig(
        Guid empresaClienteId,
        int numeroMaximoTentativas = 3,
        List<int>? intervalosDias = null,
        bool enviarEmail = true,
        bool enviarSMS = false,
        bool enviarNotificacaoInApp = true,
        int diasSuspensao = 15,
        int diasCancelamento = 30,
        TimeSpan? horaInicioRetry = null,
        TimeSpan? horaFimRetry = null)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (numeroMaximoTentativas <= 0)
            throw new ArgumentException("Número de tentativas deve ser maior que zero", nameof(numeroMaximoTentativas));

        if (diasSuspensao < 0)
            throw new ArgumentException("Dias de suspensão não pode ser negativo", nameof(diasSuspensao));

        if (diasCancelamento < 0)
            throw new ArgumentException("Dias de cancelamento não pode ser negativo", nameof(diasCancelamento));

        EmpresaClienteId = empresaClienteId;
        NumeroMaximoTentativas = numeroMaximoTentativas;
        IntervalosDias = intervalosDias ?? new List<int> { 1, 3, 7 };
        EnviarEmail = enviarEmail;
        EnviarSMS = enviarSMS;
        EnviarNotificacaoInApp = enviarNotificacaoInApp;
        DiasSuspensao = diasSuspensao;
        DiasCancelamento = diasCancelamento;
        HoraInicioRetry = horaInicioRetry ?? new TimeSpan(8, 0, 0);
        HoraFimRetry = horaFimRetry ?? new TimeSpan(20, 0, 0);
        Ativo = true;
    }

    public void AtualizarTentativas(int numeroMaximo, List<int> intervalos)
    {
        if (numeroMaximo <= 0)
            throw new ArgumentException("Número de tentativas deve ser maior que zero", nameof(numeroMaximo));

        NumeroMaximoTentativas = numeroMaximo;
        IntervalosDias = intervalos ?? throw new ArgumentNullException(nameof(intervalos));
        AtualizarDataModificacao();
    }

    public void AtualizarCanaisComunicacao(bool email, bool sms, bool inApp)
    {
        EnviarEmail = email;
        EnviarSMS = sms;
        EnviarNotificacaoInApp = inApp;
        AtualizarDataModificacao();
    }

    public void AtualizarPrazos(int diasSuspensao, int diasCancelamento)
    {
        if (diasSuspensao < 0)
            throw new ArgumentException("Dias de suspensão não pode ser negativo", nameof(diasSuspensao));

        if (diasCancelamento < 0)
            throw new ArgumentException("Dias de cancelamento não pode ser negativo", nameof(diasCancelamento));

        DiasSuspensao = diasSuspensao;
        DiasCancelamento = diasCancelamento;
        AtualizarDataModificacao();
    }

    public void AtualizarHorariosRetry(TimeSpan horaInicio, TimeSpan horaFim)
    {
        HoraInicioRetry = horaInicio;
        HoraFimRetry = horaFim;
        AtualizarDataModificacao();
    }

    public void Ativar()
    {
        Ativo = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativo = false;
        AtualizarDataModificacao();
    }

    public int ObterIntervaloDias(int numeroTentativa)
    {
        if (numeroTentativa <= 0 || numeroTentativa > IntervalosDias.Count)
            return IntervalosDias.LastOrDefault();

        return IntervalosDias[numeroTentativa - 1];
    }

    public bool PodeExecutarRetryAgora()
    {
        var horaAtual = DateTime.UtcNow.TimeOfDay;
        return horaAtual >= HoraInicioRetry && horaAtual <= HoraFimRetry;
    }
}

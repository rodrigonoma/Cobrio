namespace Cobrio.Domain.Enums;

public enum StatusFatura
{
    Pendente = 1,
    AguardandoPagamento = 2,
    Pago = 3,
    Falhou = 4,
    Cancelado = 5,
    Reembolsado = 6
}

namespace Cobrio.Domain.Enums;

public enum StatusImportacao
{
    Sucesso = 1,        // Todas as linhas processadas com sucesso
    Parcial = 2,        // Algumas linhas processadas, outras com erro
    Erro = 3            // Nenhuma linha processada
}

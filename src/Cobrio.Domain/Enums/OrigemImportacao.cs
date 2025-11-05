namespace Cobrio.Domain.Enums;

public enum OrigemImportacao
{
    Excel = 1,      // Importação via arquivo Excel
    Webhook = 2,    // Importação via Webhook/API
    Manual = 3,     // Criação manual individual
    Json = 4        // Importação em massa via JSON
}

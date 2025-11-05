namespace Cobrio.Domain.Enums;

/// <summary>
/// Tipo de ação no sistema
/// </summary>
public enum TipoAcao
{
    /// <summary>
    /// Ação de visualizar menu/módulo
    /// </summary>
    Menu = 1,

    /// <summary>
    /// Ações CRUD (Create, Read, Update, Delete)
    /// </summary>
    CRUD = 2,

    /// <summary>
    /// Ações especiais/customizadas
    /// </summary>
    Especial = 3
}

-- ============================================================================
-- SQL PARA ADICIONAR MÓDULO "RELATÓRIO DE CONSUMO" NA TELA DE PERMISSÕES
-- ============================================================================
-- Execute este script no VPS usando:
-- mysql -h 72.60.63.64 -u cobrio_user -p cobrio < adicionar-modulo-relatorio-consumo.sql
-- ============================================================================

-- Primeiro, verificar se o módulo já existe
SELECT 'Verificando se módulo já existe...' AS status;

SELECT COUNT(*) as existe
FROM Modulo
WHERE Chave = 'relatorio-consumo';

-- Se não existir (COUNT = 0), inserir o novo módulo
-- IMPORTANTE: Execute apenas se a consulta acima retornar 0

INSERT INTO Modulo (
    Id,
    Nome,
    Chave,
    NomeAmigavel,
    Descricao,
    Ordem,
    Ativo,
    CriadoEm,
    ModificadoEm
)
SELECT
    UUID() AS Id,
    'Relatório de Consumo' AS Nome,
    'relatorio-consumo' AS Chave,
    'Consumo de Canais' AS NomeAmigavel,
    'Acompanhamento do consumo de canais de notificação (Email, SMS, WhatsApp) por período, usuário e régua' AS Descricao,
    COALESCE(MAX(Ordem), 0) + 1 AS Ordem,
    1 AS Ativo,
    NOW() AS CriadoEm,
    NOW() AS ModificadoEm
FROM Modulo
WHERE NOT EXISTS (
    SELECT 1 FROM Modulo WHERE Chave = 'relatorio-consumo'
);

-- Verificar se foi inserido
SELECT 'Módulo inserido com sucesso!' AS status;

SELECT
    Id,
    Nome,
    Chave,
    NomeAmigavel,
    Ordem,
    Ativo,
    CriadoEm
FROM Modulo
WHERE Chave = 'relatorio-consumo';

-- ============================================================================
-- PRÓXIMO PASSO: Atribuir permissões para o perfil Admin
-- ============================================================================

-- Buscar o ID do módulo recém-criado
SET @moduloId = (SELECT Id FROM Modulo WHERE Chave = 'relatorio-consumo');

SELECT CONCAT('ID do módulo: ', @moduloId) AS info;

-- Criar permissões para todas as ações (create, read, update, delete)
-- Você pode ajustar conforme necessário

-- Permissão READ (visualizar o relatório)
INSERT INTO Permissao (Id, Perfil, ModuloId, Acao, Permitido, CriadoEm, ModificadoEm)
SELECT
    UUID(),
    'Admin' AS Perfil,
    @moduloId AS ModuloId,
    'read' AS Acao,
    1 AS Permitido,
    NOW() AS CriadoEm,
    NOW() AS ModificadoEm
WHERE NOT EXISTS (
    SELECT 1 FROM Permissao
    WHERE Perfil = 'Admin'
    AND ModuloId = @moduloId
    AND Acao = 'read'
);

-- Verificar permissões criadas
SELECT 'Permissões criadas com sucesso!' AS status;

SELECT
    p.Id,
    p.Perfil,
    m.Nome AS Modulo,
    m.Chave AS ModuloChave,
    p.Acao,
    p.Permitido
FROM Permissao p
INNER JOIN Modulo m ON p.ModuloId = m.Id
WHERE m.Chave = 'relatorio-consumo';

-- ============================================================================
-- RESUMO FINAL
-- ============================================================================
SELECT 'RESUMO DA EXECUÇÃO' AS '============================';

SELECT
    (SELECT COUNT(*) FROM Modulo WHERE Chave = 'relatorio-consumo') AS 'Módulo Criado',
    (SELECT COUNT(*) FROM Permissao p
     INNER JOIN Modulo m ON p.ModuloId = m.Id
     WHERE m.Chave = 'relatorio-consumo') AS 'Permissões Criadas';

SELECT 'Pronto! Agora o módulo "Relatório de Consumo" deve aparecer na tela de Gerenciar Permissões.' AS resultado;

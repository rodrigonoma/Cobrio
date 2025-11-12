-- ============================================================================
-- SQL CORRETO PARA ADICIONAR MÓDULO "RELATÓRIO DE CONSUMO"
-- ============================================================================
-- Execute estes comandos na sessão MySQL que você abriu
-- ============================================================================

-- 1. Verificar módulos de relatórios existentes
SELECT Id, Nome, Chave, Ordem, Ativo
FROM Modulo
WHERE Chave LIKE '%relatorio%'
ORDER BY Ordem;

-- 2. Inserir o novo módulo "Relatório de Consumo"
INSERT INTO Modulo (Id, Nome, Chave, Descricao, Icone, Rota, Ordem, Ativo, CriadoEm, ModificadoEm)
SELECT
    UUID() AS Id,
    'Relatório de Consumo' AS Nome,
    'relatorio-consumo' AS Chave,
    'Acompanhamento do consumo de canais de notificação (Email, SMS, WhatsApp) por período, usuário e régua' AS Descricao,
    'pi-chart-line' AS Icone,
    '/relatorios' AS Rota,
    COALESCE(MAX(Ordem), 0) + 1 AS Ordem,
    1 AS Ativo,
    NOW() AS CriadoEm,
    NOW() AS ModificadoEm
FROM Modulo
WHERE NOT EXISTS (SELECT 1 FROM Modulo WHERE Chave = 'relatorio-consumo');

-- 3. Verificar se foi inserido
SELECT 'Módulo criado com sucesso!' AS Status;
SELECT Id, Nome, Chave, Descricao, Icone, Rota, Ordem, Ativo
FROM Modulo
WHERE Chave = 'relatorio-consumo';

-- 4. Criar permissão READ para o perfil Admin
SET @moduloId = (SELECT Id FROM Modulo WHERE Chave = 'relatorio-consumo');

INSERT INTO Permissao (Id, Perfil, ModuloId, Acao, Permitido, CriadoEm, ModificadoEm)
SELECT
    UUID() AS Id,
    'Admin' AS Perfil,
    @moduloId AS ModuloId,
    'read' AS Acao,
    1 AS Permitido,
    NOW() AS CriadoEm,
    NOW() AS ModificadoEm
WHERE NOT EXISTS (
    SELECT 1 FROM Permissao
    WHERE Perfil = 'Admin' AND ModuloId = @moduloId AND Acao = 'read'
);

-- 5. Verificar permissões criadas
SELECT 'Permissões criadas com sucesso!' AS Status;
SELECT p.Id, p.Perfil, m.Nome AS Modulo, m.Chave, p.Acao, p.Permitido
FROM Permissao p
INNER JOIN Modulo m ON p.ModuloId = m.Id
WHERE m.Chave = 'relatorio-consumo';

-- 6. Listar TODOS os módulos de relatórios (deve mostrar 3 agora)
SELECT 'TODOS os módulos de relatórios:' AS Status;
SELECT Id, Nome, Chave, Ordem, Ativo
FROM Modulo
WHERE Chave LIKE '%relatorio%'
ORDER BY Ordem;

-- ============================================================================
-- PRONTO! Agora vá em "Configurações → Gerenciar Permissões"
-- Você deve ver o módulo "Relatório de Consumo" na lista!
-- ============================================================================

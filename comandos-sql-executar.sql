-- ============================================================================
-- EXECUTE ESTES COMANDOS NO MYSQL QUE VOCÊ ABRIU
-- ============================================================================

-- 1. Primeiro, verificar módulos de relatórios existentes
SELECT Id, Nome, Chave, NomeAmigavel, Ordem, Ativo
FROM Modulo
WHERE Chave LIKE '%relatorio%'
ORDER BY Ordem;

-- 2. Inserir o módulo "Relatório de Consumo" (se não existir)
INSERT INTO Modulo (Id, Nome, Chave, NomeAmigavel, Descricao, Ordem, Ativo, CriadoEm, ModificadoEm)
SELECT
    UUID(),
    'Relatório de Consumo',
    'relatorio-consumo',
    'Consumo de Canais',
    'Acompanhamento do consumo de canais de notificação (Email, SMS, WhatsApp)',
    COALESCE(MAX(Ordem), 0) + 1,
    1,
    NOW(),
    NOW()
FROM Modulo
WHERE NOT EXISTS (SELECT 1 FROM Modulo WHERE Chave = 'relatorio-consumo');

-- 3. Verificar se foi inserido
SELECT 'Módulo criado:' AS status;
SELECT Id, Nome, Chave, NomeAmigavel, Ordem, Ativo
FROM Modulo
WHERE Chave = 'relatorio-consumo';

-- 4. Criar permissão READ para o perfil Admin
SET @moduloId = (SELECT Id FROM Modulo WHERE Chave = 'relatorio-consumo');

INSERT INTO Permissao (Id, Perfil, ModuloId, Acao, Permitido, CriadoEm, ModificadoEm)
SELECT
    UUID(),
    'Admin',
    @moduloId,
    'read',
    1,
    NOW(),
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissao
    WHERE Perfil = 'Admin' AND ModuloId = @moduloId AND Acao = 'read'
);

-- 5. Verificar permissões criadas
SELECT 'Permissões criadas:' AS status;
SELECT p.Id, p.Perfil, m.Nome AS Modulo, p.Acao, p.Permitido
FROM Permissao p
INNER JOIN Modulo m ON p.ModuloId = m.Id
WHERE m.Chave = 'relatorio-consumo';

-- 6. Listar TODOS os módulos de relatórios (deve mostrar 3 agora)
SELECT 'Todos os módulos de relatórios:' AS status;
SELECT Id, Nome, Chave, NomeAmigavel, Ordem, Ativo
FROM Modulo
WHERE Chave LIKE '%relatorio%'
ORDER BY Ordem;

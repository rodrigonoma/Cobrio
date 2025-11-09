-- Script para verificar o estado das permissões em produção
USE Cobrio;

-- 1. Verificar se as tabelas existem e seus nomes
SELECT 'TABELAS EXISTENTES:' AS Info;
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'Cobrio'
AND table_name IN ('Modulo', 'modulo', 'Acao', 'acao', 'PermissaoPerfil', 'permissaoperfil')
ORDER BY table_name;

-- 2. Contar registros em Modulo
SELECT 'CONTAGEM DE MÓDULOS:' AS Info;
SELECT COUNT(*) as Total_Modulos FROM Modulo;

-- 3. Listar todos os módulos
SELECT 'MÓDULOS CADASTRADOS:' AS Info;
SELECT Id, Nome, Chave, Descricao, Icone, Rota, Ordem
FROM Modulo
ORDER BY Ordem;

-- 4. Contar registros em Acao
SELECT 'CONTAGEM DE AÇÕES:' AS Info;
SELECT COUNT(*) as Total_Acoes FROM Acao;

-- 5. Listar todas as ações
SELECT 'AÇÕES CADASTRADAS:' AS Info;
SELECT Id, Nome, Chave, Descricao, Tipo
FROM Acao
ORDER BY Tipo, Nome;

-- 6. Contar registros em PermissaoPerfil
SELECT 'CONTAGEM DE PERMISSÕES:' AS Info;
SELECT COUNT(*) as Total_Permissoes FROM PermissaoPerfil;

-- 7. Contar permissões por perfil
SELECT 'PERMISSÕES POR PERFIL:' AS Info;
SELECT
    Perfil,
    COUNT(*) as Total_Permissoes,
    SUM(CASE WHEN Permitido = 1 THEN 1 ELSE 0 END) as Permitidas,
    SUM(CASE WHEN Permitido = 0 THEN 1 ELSE 0 END) as Negadas
FROM PermissaoPerfil
GROUP BY Perfil;

-- 8. Verificar módulos que deveriam existir mas não existem
SELECT 'MÓDULOS FALTANDO:' AS Info;
SELECT 'Os seguintes módulos deveriam existir:' AS Status;
SELECT * FROM (
    SELECT 'dashboard' AS Chave_Esperada UNION ALL
    SELECT 'assinaturas' UNION ALL
    SELECT 'planos' UNION ALL
    SELECT 'regras-cobranca' UNION ALL
    SELECT 'usuarios' UNION ALL
    SELECT 'templates' UNION ALL
    SELECT 'relatorios' UNION ALL
    SELECT 'relatorios-operacionais' UNION ALL
    SELECT 'relatorios-gerenciais' UNION ALL
    SELECT 'configuracoes' UNION ALL
    SELECT 'permissoes'
) AS expected
WHERE NOT EXISTS (
    SELECT 1 FROM Modulo WHERE Chave = expected.Chave_Esperada
);

-- 9. Verificar ações que deveriam existir mas não existem
SELECT 'AÇÕES FALTANDO:' AS Info;
SELECT 'As seguintes ações deveriam existir:' AS Status;
SELECT * FROM (
    SELECT 'menu.view' AS Chave_Esperada UNION ALL
    SELECT 'read' UNION ALL
    SELECT 'visualizar' UNION ALL
    SELECT 'read.details' UNION ALL
    SELECT 'create' UNION ALL
    SELECT 'update' UNION ALL
    SELECT 'delete' UNION ALL
    SELECT 'toggle' UNION ALL
    SELECT 'export' UNION ALL
    SELECT 'import' UNION ALL
    SELECT 'reset-password' UNION ALL
    SELECT 'config-permissions'
) AS expected
WHERE NOT EXISTS (
    SELECT 1 FROM Acao WHERE Chave = expected.Chave_Esperada
);

-- 10. Verificar permissões do perfil Admin
SELECT 'PERMISSÕES DO ADMIN:' AS Info;
SELECT
    m.Nome AS Modulo,
    a.Nome AS Acao,
    pp.Permitido
FROM PermissaoPerfil pp
INNER JOIN Modulo m ON pp.ModuloId = m.Id
INNER JOIN Acao a ON pp.AcaoId = a.Id
WHERE pp.Perfil = 'Admin'
ORDER BY m.Ordem, a.Nome;

-- 11. Verificar se há algum usuário proprietário
SELECT 'USUÁRIOS PROPRIETÁRIOS:' AS Info;
SELECT Id, Nome, Email, Perfil, EhProprietario
FROM UsuarioEmpresa
WHERE EhProprietario = 1;

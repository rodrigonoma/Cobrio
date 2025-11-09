-- Verificar se a tabela TemplateEmail existe
USE cobriodb;

SELECT 'VERIFICANDO TABELA TemplateEmail:' AS Info;
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'cobriodb'
AND (table_name = 'TemplateEmail' OR table_name = 'templateemail')
ORDER BY table_name;

-- Se não aparecer nada acima, a tabela não existe e você precisa aplicar a migration
-- Execute: dotnet ef database update --startup-project src/Cobrio.API --project src/Cobrio.Infrastructure

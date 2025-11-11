-- Script para re-extrair variáveis do SubjectEmail em regras existentes
-- Este script atualiza o campo AtualizadoEm para forçar o backend a re-processar as variáveis

-- Atualizar todas as regras que têm SubjectEmail preenchido
-- Isso fará com que a próxima vez que você editar a regra no frontend,
-- as variáveis serão re-extraídas corretamente

UPDATE RegraCobranca
SET AtualizadoEm = NOW()
WHERE SubjectEmail IS NOT NULL
  AND SubjectEmail != '';

-- Verificar as regras que foram atualizadas
SELECT
    Id,
    Nome,
    SubjectEmail,
    VariaveisObrigatorias,
    AtualizadoEm
FROM RegraCobranca
WHERE SubjectEmail IS NOT NULL
  AND SubjectEmail != ''
ORDER BY AtualizadoEm DESC;

-- IMPORTANTE: Após executar este script, você precisa:
-- 1. Reiniciar a API backend (se estiver rodando)
-- 2. Abrir cada regra no frontend e clicar em "Salvar"
--    (isso forçará a re-extração das variáveis com o novo código)
--
-- OU
--
-- Execute o script abaixo para forçar uma atualização manual:

-- NOTA: MySQL não suporta funções complexas de regex para extrair grupos
-- Então a melhor solução é re-salvar as regras através da API ou frontend
-- após o backend estar atualizado com o novo código

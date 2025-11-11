-- Script para corrigir a regra "Envio Imediato" em produção

-- 1. VERIFICAR o estado atual da regra
SELECT
    Id,
    Nome,
    EhPadrao,
    TipoMomento,
    ValorTempo,
    UnidadeTempo,
    CASE UnidadeTempo
        WHEN 1 THEN 'Minutos'
        WHEN 2 THEN 'Horas'
        WHEN 3 THEN 'Dias'
        ELSE 'Desconhecido'
    END AS UnidadeTexto,
    SubjectEmail
FROM RegraCobranca
WHERE Nome = 'Envio Imediato' OR EhPadrao = 1;

-- 2. CORRIGIR a regra para ter UnidadeTempo = Minutos (1)
UPDATE RegraCobranca
SET
    UnidadeTempo = 1,  -- Minutos
    ValorTempo = 1,    -- 1 minuto
    TipoMomento = 0,   -- Antes
    AtualizadoEm = NOW()
WHERE Nome = 'Envio Imediato' OR EhPadrao = 1;

-- 3. VERIFICAR o resultado após a correção
SELECT
    Id,
    Nome,
    EhPadrao,
    TipoMomento,
    ValorTempo,
    UnidadeTempo,
    CASE UnidadeTempo
        WHEN 1 THEN 'Minutos'
        WHEN 2 THEN 'Horas'
        WHEN 3 THEN 'Dias'
        ELSE 'Desconhecido'
    END AS UnidadeTexto,
    SubjectEmail,
    AtualizadoEm
FROM RegraCobranca
WHERE Nome = 'Envio Imediato' OR EhPadrao = 1;

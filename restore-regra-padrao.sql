-- Script para restaurar a Regra de Cobrança Padrão (Envio Imediato)

-- Obter o EmpresaClienteId (você pode precisar ajustar isso)
SET @empresa_id = (SELECT Id FROM EmpresasCliente WHERE Nome = 'Empresa Demo Ltda' LIMIT 1);

-- Deletar a regra padrão existente (se houver)
DELETE FROM RegrasCobranca WHERE EmpresaClienteId = @empresa_id AND EhPadrao = 1;

-- Inserir nova regra padrão
INSERT INTO RegraCobranca (
    Id,
    EmpresaClienteId,
    Nome,
    Descricao,
    Ativa,
    EhPadrao,
    TipoMomento,
    ValorTempo,
    UnidadeTempo,
    CanalNotificacao,
    TemplateNotificacao,
    SubjectEmail,
    VariaveisObrigatorias,
    VariaveisObrigatoriasSistema,
    TokenWebhook,
    CriadoEm,
    AtualizadoEm
) VALUES (
    UUID(),
    @empresa_id,
    'Envio Imediato',
    'Regra padrão para envio imediato de cobranças sem considerar data de vencimento',
    1,
    1,
    0, -- TipoMomento.Antes
    1,
    0, -- UnidadeTempo.Minutos
    0, -- CanalNotificacao.Email
    'Prezado(a) {{nome}},\n\nEste é um lembrete sobre a cobrança no valor de {{valor}}.\n\nAtenciosamente,\nEquipe de Cobrança',
    'Lembrete de Cobrança - {{valor}}',
    '["nome","valor"]',
    '["Email"]',
    REPLACE(UUID(), '-', ''),
    NOW(),
    NOW()
);

-- Verificar se foi criada
SELECT * FROM RegrasCobranca WHERE EmpresaClienteId = @empresa_id AND EhPadrao = 1;

-- DIAGNÓSTICO COMPLETO - Encontrar o histórico para o email de abertura

-- 1. Verificar se existe algum histórico para rodrigonoma@gmail.com nas últimas 2 horas
SELECT
    'Históricos recentes' as Tipo,
    Id,
    MessageIdProvedor,
    Status,
    DataEnvio,
    QuantidadeAberturas,
    SUBSTRING(PayloadUtilizado, 1, 200) as PayloadPreview
FROM HistoricoNotificacao
WHERE DataEnvio >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
  AND CanalUtilizado = 1
ORDER BY DataEnvio DESC
LIMIT 10;

-- 2. Buscar especificamente pelo MessageId que veio no webhook
SELECT
    'Busca por MessageId exato' as Tipo,
    Id,
    MessageIdProvedor,
    Status,
    DataEnvio,
    QuantidadeAberturas
FROM HistoricoNotificacao
WHERE MessageIdProvedor = '<202511120105.17350695709@smtp-relay.mailin.fr>';

-- 3. Buscar pelo ID numérico
SELECT
    'Busca por ID numérico' as Tipo,
    Id,
    MessageIdProvedor,
    Status,
    DataEnvio,
    QuantidadeAberturas
FROM HistoricoNotificacao
WHERE MessageIdProvedor = '1636105';

-- 4. Buscar qualquer histórico que contenha parte do MessageId
SELECT
    'Busca por LIKE' as Tipo,
    Id,
    MessageIdProvedor,
    Status,
    DataEnvio,
    QuantidadeAberturas
FROM HistoricoNotificacao
WHERE MessageIdProvedor LIKE '%17350695709%'
   OR MessageIdProvedor LIKE '%202511120105%';

-- 5. Verificar PayloadUtilizado para ver se contém o email
SELECT
    'Históricos com email no payload' as Tipo,
    Id,
    MessageIdProvedor,
    Status,
    DataEnvio,
    PayloadUtilizado
FROM HistoricoNotificacao
WHERE DataEnvio >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
  AND CanalUtilizado = 1
  AND PayloadUtilizado LIKE '%rodrigonoma@gmail.com%'
ORDER BY DataEnvio DESC
LIMIT 5;

-- 6. Verificar a tabela Cobranca para encontrar cobranças com esse email
SELECT
    'Cobranças recentes' as Tipo,
    c.Id as CobrancaId,
    c.DataCriacao,
    c.PayloadJson,
    h.Id as HistoricoId,
    h.MessageIdProvedor,
    h.Status,
    h.DataEnvio
FROM Cobranca c
LEFT JOIN HistoricoNotificacao h ON c.Id = h.CobrancaId
WHERE c.DataCriacao >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
  AND c.PayloadJson LIKE '%rodrigonoma@gmail.com%'
ORDER BY c.DataCriacao DESC
LIMIT 5;

-- 7. Verificar os webhooks de request/delivered que foram bem sucedidos
SELECT
    'Webhooks bem sucedidos' as Tipo,
    EventoTipo,
    MessageId,
    HistoricoNotificacaoId,
    DataEvento,
    DataRecebimento
FROM BrevoWebhookLog
WHERE Email = 'rodrigonoma@gmail.com'
  AND ProcessadoComSucesso = 1
  AND DataRecebimento >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
ORDER BY DataRecebimento DESC;

-- 8. Pegar o HistoricoNotificacaoId do último webhook bem sucedido e ver seus dados
SELECT
    'Detalhes do último histórico processado' as Tipo,
    h.*
FROM HistoricoNotificacao h
WHERE h.Id IN (
    SELECT HistoricoNotificacaoId
    FROM BrevoWebhookLog
    WHERE Email = 'rodrigonoma@gmail.com'
      AND ProcessadoComSucesso = 1
      AND HistoricoNotificacaoId IS NOT NULL
      AND DataRecebimento >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
    LIMIT 1
);

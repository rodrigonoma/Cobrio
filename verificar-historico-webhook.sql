-- Verificar o HistoricoNotificacao que FOI encontrado
SELECT
    Id,
    MessageIdProvedor,
    Email,
    CobrancaId,
    Status,
    DataEnvio,
    QuantidadeAberturas,
    DataPrimeiraAbertura
FROM HistoricoNotificacao
WHERE Id = '790255a8-c341-43f7-bde7-876dae930987';

-- Verificar TODOS os HistoricoNotificacao para rodrigonoma@gmail.com nas últimas 24h
SELECT
    Id,
    MessageIdProvedor,
    CobrancaId,
    Status,
    DataEnvio,
    QuantidadeAberturas,
    DataPrimeiraAbertura
FROM HistoricoNotificacao
WHERE
    CanalUtilizado = 1 -- Email
    AND DataEnvio >= DATE_SUB(NOW(), INTERVAL 24 HOUR)
ORDER BY DataEnvio DESC;

-- Verificar se existe algum histórico com os MessageIds dos eventos opened
SELECT
    'Busca por MessageId',
    MessageIdProvedor,
    Id,
    Status,
    DataEnvio
FROM HistoricoNotificacao
WHERE MessageIdProvedor IN (
    '<202511120030.11379561925@smtp-relay.mailin.fr>',
    '<202511111350.22180459351@smtp-relay.mailin.fr>',
    '<202511112251.38890123152@smtp-relay.mailin.fr>',
    '<202511112316.89687255308@smtp-relay.mailin.fr>'
)
UNION ALL
SELECT
    'Busca por ID numérico',
    MessageIdProvedor,
    Id,
    Status,
    DataEnvio
FROM HistoricoNotificacao
WHERE MessageIdProvedor = '1636105';

-- Ver os logs de webhook para o MessageId específico
SELECT
    EventoTipo,
    Email,
    MessageId,
    ProcessadoComSucesso,
    MensagemErro,
    HistoricoNotificacaoId,
    DataEvento,
    PayloadCompleto
FROM BrevoWebhookLog
WHERE MessageId = '<202511120030.11379561925@smtp-relay.mailin.fr>'
ORDER BY DataEvento;

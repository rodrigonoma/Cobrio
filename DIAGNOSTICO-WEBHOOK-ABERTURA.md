# Diagnóstico: Problema com Webhook de Abertura do Brevo

## Data da Análise
12/11/2025 00:30

## Problema Reportado
Os eventos de abertura (`opened` e `unique_opened`) do Brevo estão sendo recebidos pelo webhook, salvos na tabela `BrevoWebhookLog`, mas NÃO estão aparecendo na tela de "Logs de Notificação".

## Análise dos Dados

### Registros na Tabela BrevoWebhookLog

| EventoTipo     | MessageId                                      | ProcessadoComSucesso | HistoricoNotificacaoId | MensagemErro |
|----------------|-----------------------------------------------|----------------------|------------------------|--------------|
| delivered      | `<202511120030.11379561925@smtp-relay.mailin.fr>` | 1 (Sim)           | `790255a8-...`         | NULL         |
| request        | `<202511120030.11379561925@smtp-relay.mailin.fr>` | 0 (Não)           | NULL                   | NULL         |
| **unique_opened** | `<202511120030.11379561925@smtp-relay.mailin.fr>` | **0 (Não)** | **NULL** | **NULL** |
| opened         | `<202511111350.22180459351@smtp-relay.mailin.fr>` | 0 (Não)           | NULL                   | NULL         |
| opened         | `<202511112251.38890123152@smtp-relay.mailin.fr>` | 0 (Não)           | NULL                   | NULL         |
| opened         | `<202511112316.89687255308@smtp-relay.mailin.fr>` | 0 (Não)           | NULL                   | NULL         |

### Observações Importantes

1. **Evento `unique_opened` tem o MessageId CORRETO** mas ainda assim falhou o processamento
2. **Eventos `opened` têm MessageIds de emails ANTIGOS** (provavelmente do Gmail recarregando imagens em cache)
3. **Todos os eventos têm o mesmo `BrevoEventId`: 1636105** (isso é incomum)
4. **Campo `MensagemErro` está NULL** em todos os registros que falharam (indicando versão antiga do código)

## Causas Raiz Identificadas

### 1. Aplicação Rodando Versão Antiga
A aplicação no servidor ainda está executando a versão anterior do código que:
- Não salvava o log do webhook no início do processamento
- Não atualizava o campo `MensagemErro` quando falhava
- Usava apenas UMA estratégia de busca (pelo MessageId)
- Não tinha logs detalhados para debug

### 2. Problema de Busca do Histórico
O evento `unique_opened` tem o MessageId correto, mas a busca está falham porque:
- Possível incompatibilidade entre o MessageId salvo no banco vs. o recebido no webhook
- Necessidade de múltiplas estratégias de busca (MessageId, ID numérico, Email+Data)

### 3. Eventos de Abertura de Emails Antigos
Os eventos `opened` têm MessageIds diferentes porque o Gmail recarrega imagens de emails anteriores.
Isso é comportamento normal e esperado. Apenas o `unique_opened` importa.

## Solução Implementada

### Alterações no Código

#### 1. Adicionado novo método no repositório
**Arquivo**: `src/Cobrio.Domain/Interfaces/IHistoricoNotificacaoRepository.cs`
```csharp
Task<HistoricoNotificacao?> GetByEmailEDataAsync(
    string email,
    DateTime dataReferencia,
    int toleranciaMinutos = 30,
    CancellationToken cancellationToken = default);
```

**Implementação**: `src/Cobrio.Infrastructure/Repositories/HistoricoNotificacaoRepository.cs:66-86`

#### 2. Refatorado BrevoWebhookService
**Arquivo**: `src/Cobrio.Application/Services/BrevoWebhookService.cs:68-111`

**Nova lógica de busca com 3 estratégias**:
1. **Estratégia 1**: Buscar pelo Message-ID (RFC 2822) - ex: `<202511120030.11379561925@smtp-relay.mailin.fr>`
2. **Estratégia 2**: Buscar pelo ID numérico do Brevo - ex: `1636105`
3. **Estratégia 3**: Buscar por Email + Data aproximada (±60 minutos) - fallback para casos extremos

**Melhorias adicionais**:
- Logs detalhados com emojis para facilitar debug
- Salvamento do webhook log no início (auditoria completa)
- Atualização correta do campo `MensagemErro` quando falha
- Mensagens de erro informando todas as tentativas de busca

## Próximos Passos para Resolução

### Passo 1: Reiniciar a API ⚠️ OBRIGATÓRIO
```bash
# Parar o processo da API atual
pkill -f Cobrio.API

# Ou se estiver rodando como serviço:
systemctl restart cobrio-api

# Ou no Windows:
# Parar o processo atual e iniciar novamente
```

**IMPORTANTE**: A nova versão já foi publicada em `C:\Cobrio\Cobriopublish\`, mas a API precisa ser reiniciada para carregar as alterações.

### Passo 2: Executar Script de Diagnóstico
Execute o script SQL criado para verificar os dados:
```bash
mysql -h 72.60.63.64 -u cobrio_user -p cobrio < verificar-historico-webhook.sql
```

Este script irá:
- Verificar o HistoricoNotificacao que foi processado com sucesso
- Listar todos os históricos de email nas últimas 24h
- Verificar quais MessageIds existem no banco
- Correlacionar os eventos de webhook com o histórico

### Passo 3: Testar com Novo Evento
Após reiniciar a API, envie um novo email e monitore:

1. **Verifique os logs da aplicação**:
```bash
tail -f /var/log/cobrio/api.log
# Ou onde estiverem os logs da aplicação
```

2. **Você deverá ver nos logs**:
```
🔔 Webhook Brevo recebido - Evento: unique_opened | Email: rodrigonoma@gmail.com | MessageId: <...> | Id: ...
✅ Webhook log salvo - LogId: ...
🔍 [1/3] Buscando histórico pelo Message-ID RFC 2822: '<...>'
✅ Histórico ENCONTRADO - Id: ... | CobrancaId: ... | Status atual: ...
📧 Registrando ABERTURA - Data: ... | IP: ... | UserAgent: ...
✅ Abertura registrada - Qtd aberturas: 1 | Novo status: Aberto
```

3. **Consultar a tabela BrevoWebhookLog**:
```sql
SELECT
    EventoTipo,
    Email,
    ProcessadoComSucesso,
    MensagemErro,
    HistoricoNotificacaoId
FROM BrevoWebhookLog
WHERE Email = 'rodrigonoma@gmail.com'
ORDER BY DataRecebimento DESC
LIMIT 5;
```

4. **Consultar o HistoricoNotificacao**:
```sql
SELECT
    Id,
    Status,
    QuantidadeAberturas,
    DataPrimeiraAbertura,
    DataUltimaAbertura,
    IpAbertura,
    UserAgentAbertura
FROM HistoricoNotificacao
WHERE MessageIdProvedor = '<MessageId do email>'
  OR Id = '<HistoricoNotificacaoId encontrado>';
```

### Passo 4: Usar Endpoint de Teste (Opcional)
Para testar sem esperar um novo email, use o endpoint de teste:

```bash
curl -X POST https://cobrio.com.br/api/webhook/brevo/teste-abertura \
  -H "Content-Type: application/json" \
  -d '{
    "MessageId": "<202511120030.11379561925@smtp-relay.mailin.fr>",
    "Email": "rodrigonoma@gmail.com"
  }'
```

## Resultado Esperado

Após reiniciar a API:

1. ✅ Eventos de `unique_opened` serão processados com sucesso
2. ✅ Campo `MensagemErro` será preenchido quando houver falha
3. ✅ Logs detalhados permitirão debug fácil
4. ✅ Aberturas aparecerão na tela de "Logs de Notificação"
5. ✅ Contador de aberturas será incrementado
6. ✅ Status da notificação mudará para "Aberto"

## Arquivos Modificados

1. `src/Cobrio.Domain/Interfaces/IHistoricoNotificacaoRepository.cs`
2. `src/Cobrio.Infrastructure/Repositories/HistoricoNotificacaoRepository.cs`
3. `src/Cobrio.Application/Services/BrevoWebhookService.cs`

## Arquivos Criados

1. `verificar-historico-webhook.sql` - Script de diagnóstico
2. `DIAGNOSTICO-WEBHOOK-ABERTURA.md` - Este documento

## Compilação

✅ Build concluído com sucesso
✅ Publish gerado em: `C:\Cobrio\Cobriopublish\`

---

**Status**: ⚠️ AGUARDANDO REINÍCIO DA API PARA APLICAR CORREÇÕES

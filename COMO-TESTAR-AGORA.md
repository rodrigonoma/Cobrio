# COMO TESTAR A CORREÇÃO DO WEBHOOK DE ABERTURA

## ✅ O QUE FOI FEITO

1. **Corrigido método de busca por Email+Data** - Agora busca tanto no PayloadUtilizado quanto no PayloadJson da Cobranca
2. **Adicionados logs EXTREMAMENTE detalhados** - Você verá exatamente o que está acontecendo em cada etapa
3. **Criado endpoint de debug** - Para testar com o payload exato do Brevo
4. **Código já compilado e publicado** em `C:\Cobrio\Cobriopublish\`

## 🚀 PASSO 1: FAZER DEPLOY NO VPS

```bash
# No seu VPS, faça o upload da pasta Cobriopublish e reinicie o PM2
pm2 restart cobrio-api
pm2 logs cobrio-api
```

## 🧪 PASSO 2: TESTAR COM O ENDPOINT DE DEBUG

Use o arquivo `teste-webhook-abertura.json` que contém o payload EXATO que você forneceu:

```bash
curl -X POST https://cobrio.com.br/api/webhook/brevo/debug-payload \
  -H "Content-Type: application/json" \
  -d @teste-webhook-abertura.json
```

## 📊 PASSO 3: VERIFICAR OS LOGS

Monitore os logs do PM2:

```bash
pm2 logs cobrio-api --lines 100
```

Você DEVERÁ ver algo assim:

```
🔔 Webhook Brevo recebido - Evento: unique_opened | Email: rodrigonoma@gmail.com | MessageId: <202511120105.17350695709@smtp-relay.mailin.fr> | Id: 1636105
✅ Webhook log salvo - LogId: ...
🎯 INICIANDO BUSCA DO HISTÓRICO | MessageId: '<202511120105.17350695709@smtp-relay.mailin.fr>' | Id: 1636105 | Email: rodrigonoma@gmail.com
🔍 [1/3] Buscando histórico pelo Message-ID RFC 2822: '<202511120105.17350695709@smtp-relay.mailin.fr>'
```

E então:
- **SE ENCONTRAR**: `✅ [1/3] ENCONTRADO pelo Message-ID! HistoricoId: ...`
- **SE NÃO ENCONTRAR**: `❌ [1/3] NÃO encontrado pelo Message-ID`
- Depois tenta o ID numérico: `🔍 [2/3] Buscando histórico pelo ID numérico Brevo: '1636105'`
- Se ainda não encontrar: `🔍 [3/3] Buscando histórico por Email + Data (fallback): 'rodrigonoma@gmail.com' perto de 2025-11-12 01:05 (tolerância: ±60 min)`

## 🔍 PASSO 4: INVESTIGAR O BANCO DE DADOS

Execute o script SQL que criei:

```bash
# No VPS ou localmente com acesso ao MySQL
mysql -h SEU_HOST -u SEU_USER -p SEU_DATABASE < debug-webhook-agora.sql
```

Este script irá mostrar:
- Todos os históricos recentes
- Busca pelo MessageId exato
- Busca pelo ID numérico
- Busca por LIKE
- Históricos com o email no payload
- Cobranças recentes
- Webhooks bem sucedidos

## 📋 POSSÍVEIS CENÁRIOS E SOLUÇÕES

### Cenário 1: Logs mostram "✅ ENCONTRADO" mas ainda não aparece na tela

**Problema**: O histórico foi encontrado e atualizado, mas a tela não mostra.

**Solução**: Verifique se o frontend está filtrando os resultados ou se há cache. Force um refresh da página.

### Cenário 2: Logs mostram "❌ NÃO encontrado" em todas as 3 tentativas

**Problema**: O histórico realmente não existe no banco OU o MessageIdProvedor não foi salvo.

**Soluções**:
1. Execute o SQL `debug-webhook-agora.sql` para ver o que tem no banco
2. Verifique se o evento `request` ou `delivered` foi processado ANTES do `unique_opened`
3. O MessageIdProvedor pode estar em formato diferente

**Se o histórico não existir no banco**, o problema é que o email não foi enviado ainda ou não foi salvo. Neste caso:

```sql
-- Verificar se existem históricos SEM MessageIdProvedor
SELECT Id, CobrancaId, DataEnvio, MessageIdProvedor, Status
FROM HistoricoNotificacao
WHERE DataEnvio >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
  AND CanalUtilizado = 1
  AND MessageIdProvedor IS NULL;
```

### Cenário 3: "✅ [3/3] ENCONTRADO por Email+Data"

**Problema**: As duas primeiras buscas falharam, mas a terceira (Email+Data) funcionou.

**Solução**: Isso significa que o MessageIdProvedor não está sendo salvo corretamente. Verifique:
1. Se o evento `request` ou `delivered` está sendo processado
2. Se o método `RegistrarMessageId` está sendo chamado

## 🔧 SE AINDA NÃO FUNCIONAR

### Opção A: Investigar qual MessageIdProvedor está salvo

```sql
-- Ver exatamente o que está salvo no campo MessageIdProvedor
SELECT
    Id,
    MessageIdProvedor,
    HEX(MessageIdProvedor) as MessageIdHex,
    LENGTH(MessageIdProvedor) as Comprimento,
    DataEnvio
FROM HistoricoNotificacao
WHERE DataEnvio >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
ORDER BY DataEnvio DESC
LIMIT 5;
```

### Opção B: Atualizar manualmente para testar

```sql
-- Pegar o ID do histórico mais recente
SET @historico_id = (
    SELECT Id
    FROM HistoricoNotificacao
    WHERE DataEnvio >= DATE_SUB(NOW(), INTERVAL 2 HOUR)
      AND CanalUtilizado = 1
    ORDER BY DataEnvio DESC
    LIMIT 1
);

-- Atualizar com o MessageId do webhook
UPDATE HistoricoNotificacao
SET MessageIdProvedor = '<202511120105.17350695709@smtp-relay.mailin.fr>'
WHERE Id = @historico_id;

-- Agora testar novamente com o curl acima
```

### Opção C: Forçar o salvamento do MessageIdProvedor

Adicione este código no `ProcessarCobrancasJob.cs` logo após salvar o histórico:

```csharp
// Logo após criar o histórico, salvar o MessageIdProvedor imediatamente
if (!string.IsNullOrWhiteSpace(resultado.IdRastreamento))
{
    historico.RegistrarMessageId(resultado.IdRastreamento);
    await _unitOfWork.CommitAsync(cancellationToken); // IMPORTANTE: Commit imediatamente
}
```

## 📞 INFORMAÇÕES PARA DEBUG

Quando você testar, me envie:

1. **Os logs completos** do PM2 (últimas 50 linhas)
2. **O resultado do SQL** `debug-webhook-agora.sql`
3. **O response do curl** do teste

Assim posso ver exatamente onde está travando.

## 📁 ARQUIVOS CRIADOS PARA VOCÊ

1. **`teste-webhook-abertura.json`** - Payload exato para teste
2. **`debug-webhook-agora.sql`** - Script SQL de diagnóstico completo
3. **`COMO-TESTAR-AGORA.md`** - Este guia
4. **`verificar-historico-webhook.sql`** - Script SQL alternativo
5. **`DIAGNOSTICO-WEBHOOK-ABERTURA.md`** - Documentação completa do problema

## 🎯 ENDPOINT NOVO CRIADO

`POST /api/webhook/brevo/debug-payload` - Aceita o payload EXATO do Brevo e retorna sucesso/falha

```bash
# Teste rápido
curl -X POST https://cobrio.com.br/api/webhook/brevo/debug-payload \
  -H "Content-Type: application/json" \
  -d '{
    "event": "unique_opened",
    "email": "rodrigonoma@gmail.com",
    "id": 1636105,
    "message-id": "<202511120105.17350695709@smtp-relay.mailin.fr>",
    "ts_event": 1762909533
  }'
```

---

**IMPORTANTE**: Depois de fazer o deploy, MONITORE OS LOGS em tempo real enquanto testa!

```bash
pm2 logs cobrio-api --lines 100 --raw
```

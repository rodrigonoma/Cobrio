# Configuração do Ambiente de Desenvolvimento

## 🔐 Configuração de Secrets (API Keys)

**IMPORTANTE:** Nunca commite API keys no Git! O Brevo e outros serviços revogam automaticamente keys expostas em repositórios.

### Passo a passo:

1. **Copie o arquivo de exemplo:**
   ```bash
   cd src/Cobrio.API
   copy appsettings.Development.example.json appsettings.Development.json
   ```

2. **Configure suas API keys no arquivo `appsettings.Development.json`:**
   - `Brevo.ApiKey`: Sua chave de API do Brevo (para envio de emails)
   - `Twilio.AccountSid`: Seu Account SID do Twilio (para SMS/WhatsApp)
   - `Twilio.AuthToken`: Seu Auth Token do Twilio

3. **O arquivo `appsettings.Development.json` está no `.gitignore`**, então suas keys ficarão apenas na sua máquina local.

### Obtendo as API Keys:

- **Brevo (Email):** https://app.brevo.com/settings/keys/api
- **Twilio (SMS/WhatsApp):** https://console.twilio.com/

### Estrutura de configuração:

```json
{
  "Brevo": {
    "ApiKey": "xkeysib-...",
    "FromEmail": "seu-email@dominio.com",
    "FromName": "Cobrio - Sistema de Cobrança"
  },
  "Twilio": {
    "AccountSid": "AC...",
    "AuthToken": "...",
    "NumeroRemetente": "+5511999999999",
    "NumeroWhatsApp": "+14155238886"
  }
}
```

## 🗄️ Configuração do Banco de Dados

1. **MySQL:**
   ```bash
   # Instale o MySQL 8.0 ou superior
   # Configure usuário root com senha root (ou ajuste no appsettings.json)
   ```

2. **Redis:**
   ```bash
   # Instale o Redis localmente
   # Porta padrão: 6379
   ```

3. **Migrations:**
   ```bash
   cd src/Cobrio.Infrastructure
   dotnet ef database update --startup-project ../Cobrio.API
   ```

## ▶️ Executando a aplicação

### Backend:
```bash
cd src/Cobrio.API
dotnet run
# Acesse: http://localhost:5271
```

### Frontend:
```bash
cd cobrio-web
npm install
npm start
# Acesse: http://localhost:4201
```

## 🔒 Segurança

- ✅ Arquivo `.gitignore` configurado para ignorar arquivos com secrets
- ✅ Arquivo de exemplo (`appsettings.Development.example.json`) commitado
- ✅ Arquivo real (`appsettings.Development.json`) **NÃO** é commitado
- ❌ **NUNCA** commite API keys, tokens ou senhas no Git

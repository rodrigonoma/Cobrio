# Cobrio - Plataforma de Automação de Cobrança Recorrente

![Status](https://img.shields.io/badge/status-em%20desenvolvimento-yellow)
![.NET](https://img.shields.io/badge/.NET-7.0+-purple)
![Angular](https://img.shields.io/badge/Angular-16+-red)
![MySQL](https://img.shields.io/badge/MySQL-8.0+-blue)

Plataforma SaaS multi-tenant para automação de régua de cobrança, gestão de inadimplência e otimização de recebíveis para negócios de assinatura.

---

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Executando o Projeto](#executando-o-projeto)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Roadmap](#roadmap)

---

## 🎯 Sobre o Projeto

**Cobrio** é uma plataforma SaaS que automatiza todo o ciclo de cobrança recorrente, reduzindo churn involuntário através de:

- ✅ **Cobrança Automática**: Renovação e processamento de pagamentos
- ✅ **Dunning Inteligente**: Régua de cobrança configurável com retry automático
- ✅ **Multi-gateway**: Suporte para múltiplos provedores de pagamento
- ✅ **Dashboards**: Métricas em tempo real (MRR, ARR, Churn, Recuperação)
- ✅ **Portal do Assinante**: Auto-atendimento para atualização de pagamento
- ✅ **Multi-tenant**: Isolamento completo de dados por cliente

**Público-alvo**: Startups, SaaS, serviços de assinatura que precisam automatizar cobrança e reduzir inadimplência.

---

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** com DDD pragmático:

```
┌─────────────────────────────────────────┐
│   Presentation (API + Angular SPA)     │
├─────────────────────────────────────────┤
│   Application (Services + DTOs)        │
├─────────────────────────────────────────┤
│   Domain (Entities + Business Logic)   │
├─────────────────────────────────────────┤
│   Infrastructure (DB + External APIs)  │
└─────────────────────────────────────────┘
```

**Princípios**:
- Separação de responsabilidades
- Injeção de dependência
- Testabilidade
- Performance-first
- Multi-tenancy by design

📖 [Documentação Completa da Arquitetura](docs/arquitetura-tecnica.md)

---

## 🚀 Tecnologias

### Backend
- **Framework**: .NET 7+ (C#)
- **ORM**: Entity Framework Core 7+
- **Database**: MySQL 8+
- **Cache**: Redis + In-Memory Cache
- **Background Jobs**: Hangfire
- **Authentication**: JWT Bearer
- **Logging**: Serilog
- **Validation**: FluentValidation
- **Mapping**: AutoMapper

### Frontend
- **Framework**: Angular 16+
- **UI Components**: PrimeNG + Tailwind CSS
- **State**: RxJS (Reactive Programming)
- **Charts**: ApexCharts
- **HTTP**: HttpClient + Interceptors
- **Build**: Angular CLI

### DevOps
- **CI/CD**: GitHub Actions / Azure DevOps
- **Hosting**: IIS / Azure App Service
- **Monitoring**: Application Insights / Serilog

---

## 📦 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

### Backend
- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) ou superior
- [MySQL 8.0+](https://dev.mysql.com/downloads/)
- [Redis](https://redis.io/download) (opcional para desenvolvimento)

### Frontend
- [Node.js 18+](https://nodejs.org/) (LTS recomendado)
- [npm](https://www.npmjs.com/) ou [yarn](https://yarnpkg.com/)
- [Angular CLI](https://angular.io/cli) (`npm install -g @angular/cli`)

### Ferramentas
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)
- [MySQL Workbench](https://www.mysql.com/products/workbench/) (opcional)
- [Postman](https://www.postman.com/) (para testar API)

---

## 🔧 Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/cobrio.git
cd cobrio
```

### 2. Configure o banco de dados

```bash
# Criar database no MySQL
mysql -u root -p
CREATE DATABASE cobrio_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'cobrio_user'@'localhost' IDENTIFIED BY 'sua_senha_segura';
GRANT ALL PRIVILEGES ON cobrio_db.* TO 'cobrio_user'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

### 3. Instale as dependências do Backend

```bash
cd src
dotnet restore
```

### 4. Instale as dependências do Frontend

```bash
cd src/Cobrio.Web
npm install
```

---

## ⚙️ Configuração

### Backend - appsettings.json

Crie `src/Cobrio.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=cobrio_db;User=cobrio_user;Password=sua_senha;",
    "RedisConnection": "localhost:6379"
  },
  "JwtSettings": {
    "SecretKey": "sua_chave_secreta_aqui_min_256_bits",
    "Issuer": "Cobrio.API",
    "Audience": "Cobrio.Web",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "PaymentGateway": {
    "Provider": "Stripe",
    "ApiKey": "sua_api_key_teste",
    "WebhookSecret": "seu_webhook_secret"
  },
  "EmailSettings": {
    "Provider": "SendGrid",
    "ApiKey": "sua_sendgrid_key",
    "FromEmail": "noreply@cobrio.com.br",
    "FromName": "Cobrio"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Frontend - environment.ts

Edite `src/Cobrio.Web/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5001/api',
  apiVersion: 'v1'
};
```

---

## ▶️ Executando o Projeto

### Backend

```bash
# Aplicar migrations
cd src/Cobrio.API
dotnet ef database update

# Executar API
dotnet run

# API estará disponível em: https://localhost:5001
```

### Frontend

```bash
cd src/Cobrio.Web
ng serve

# App estará disponível em: http://localhost:4200
```

### Acesso inicial

- **API Swagger**: https://localhost:5001/swagger
- **Frontend**: http://localhost:4200
- **Usuário admin padrão**: será criado pelo seed (se configurado)

---

## 📁 Estrutura do Projeto

```
Cobrio/
├── src/
│   ├── Cobrio.Domain/              # Entidades, Value Objects, Interfaces
│   ├── Cobrio.Application/         # Services, DTOs, Use Cases
│   ├── Cobrio.Infrastructure/      # EF Core, Repositories, External APIs
│   ├── Cobrio.API/                 # Controllers, Middlewares
│   └── Cobrio.Web/                 # Angular SPA
├── tests/
│   ├── Cobrio.UnitTests/
│   ├── Cobrio.IntegrationTests/
│   └── Cobrio.PerformanceTests/
├── docs/
│   ├── arquitetura-tecnica.md
│   ├── modelo-dados.md
│   └── api-documentation.md
├── Cobrio.sln
└── README.md
```

---

## 🗓️ Roadmap

### MVP (Fase 1) - Q1 2025
- [x] Arquitetura e estrutura de projetos
- [ ] Modelo de dados e migrations
- [ ] Autenticação JWT multi-tenant
- [ ] CRUD de Planos e Assinantes
- [ ] Integração com 1 gateway (Stripe/PagarMe)
- [ ] Cobrança automática básica
- [ ] Retry simples (1 tentativa)
- [ ] Dashboard básico (assinantes ativos, MRR)
- [ ] Portal assinante (visualizar fatura + atualizar pagamento)

### Pós-MVP (Fase 2) - Q2 2025
- [ ] Dunning completo (múltiplas tentativas configuráveis)
- [ ] Templates de email/SMS customizáveis
- [ ] Proração de planos (upgrade/downgrade)
- [ ] Relatórios avançados (churn, recuperação, aging)
- [ ] Multi-gateway com fallback
- [ ] Webhooks para eventos
- [ ] API pública documentada

### Futuro (Fase 3) - Q3+ 2025
- [ ] Multi-moeda e impostos regionais
- [ ] Integração CRM/ERP
- [ ] Analytics avançado (ML para predição de churn)
- [ ] Portal white-label customizável
- [ ] Mobile app (PWA)

---

## 🧪 Testes

```bash
# Testes unitários
cd tests/Cobrio.UnitTests
dotnet test

# Testes de integração
cd tests/Cobrio.IntegrationTests
dotnet test

# Coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## 📊 Performance

Requisitos de performance:

- **API Response Time**: < 200ms (p95)
- **Dashboard Load**: < 1s
- **Background Jobs**: Processamento assíncrono
- **Cache Hit Rate**: > 80% para queries repetitivas
- **Database**: Índices otimizados para queries multi-tenant

---

## 🔒 Segurança

- ✅ HTTPS obrigatório
- ✅ JWT com refresh token
- ✅ CORS configurado
- ✅ Rate limiting
- ✅ SQL injection protection (EF Core parameterização)
- ✅ XSS protection (Angular sanitização)
- ✅ LGPD compliance (dados sensíveis criptografados)
- ✅ Tokenização de dados de pagamento (PCI DSS)

---

## 📄 Licença

Este projeto é proprietário. Todos os direitos reservados.

---

## 👥 Equipe

- **Tech Lead**: [Seu Nome]
- **Backend**: [Desenvolvedores]
- **Frontend**: [Desenvolvedores]
- **QA**: [Testadores]

---

## 📞 Contato

- **Email**: contato@cobrio.com.br
- **Website**: https://cobrio.com.br
- **Suporte**: suporte@cobrio.com.br

---

**Desenvolvido com ❤️ para revolucionar a cobrança recorrente no Brasil**

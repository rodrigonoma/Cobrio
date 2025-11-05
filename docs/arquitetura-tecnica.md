# Arquitetura Técnica - Cobrio

## 1. Visão Geral

**Plataforma**: SaaS Multi-tenant para Automação de Cobrança Recorrente
**Stack**: C# (.NET 7+) + Angular 16+ + MySQL 8+
**Princípios**: Clean Architecture, DDD Pragmático, SOLID, Performance-First

---

## 2. Arquitetura de Camadas

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│  ┌──────────────────┐              ┌────────────────────┐   │
│  │  Angular SPA     │              │   API REST         │   │
│  │  (Mobile-First)  │◄────────────►│   (Controllers)    │   │
│  └──────────────────┘              └────────────────────┘   │
└────────────────────────────────────────┬────────────────────┘
                                         │
┌────────────────────────────────────────▼────────────────────┐
│                    Application Layer                         │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Application Services (Use Cases)                   │    │
│  │  - PlanoService, AssinaturaService, CobrancaService │    │
│  │  - DTOs, Validators, Mappers                        │    │
│  └─────────────────────────────────────────────────────┘    │
└────────────────────────────────────────┬────────────────────┘
                                         │
┌────────────────────────────────────────▼────────────────────┐
│                      Domain Layer                            │
│  ┌──────────────┐  ┌─────────────┐  ┌──────────────────┐   │
│  │  Entities    │  │ Value Objs  │  │ Domain Services  │   │
│  │  - Assinante │  │ - Money     │  │ - CobrancaDomain │   │
│  │  - Plano     │  │ - Email     │  │ - DunningService │   │
│  │  - Fatura    │  │ - CPF/CNPJ  │  └──────────────────┘   │
│  └──────────────┘  └─────────────┘                          │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Domain Interfaces (IRepositories)                  │    │
│  └─────────────────────────────────────────────────────┘    │
└────────────────────────────────────────┬────────────────────┘
                                         │
┌────────────────────────────────────────▼────────────────────┐
│                   Infrastructure Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐   │
│  │ Repositories │  │ EF Core      │  │ External APIs   │   │
│  │ UnitOfWork   │  │ DbContext    │  │ - Payment GW    │   │
│  │ Cache        │  │ Migrations   │  │ - Email/SMS     │   │
│  └──────────────┘  └──────────────┘  └─────────────────┘   │
│  ┌────────────────────────────────────────────────────┐     │
│  │ Background Jobs (Hangfire/Quartz)                  │     │
│  │ - Geração de Faturas, Retry Pagamento, Dunning    │     │
│  └────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. Estrutura de Projetos

```
Cobrio/
├── src/
│   ├── Cobrio.Domain/                    # Core business logic
│   │   ├── Entities/
│   │   │   ├── EmpresaCliente.cs
│   │   │   ├── Assinante.cs
│   │   │   ├── Plano.cs
│   │   │   ├── Fatura.cs
│   │   │   ├── MetodoPagamento.cs
│   │   │   └── TentativaPagamento.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Email.cs
│   │   │   ├── CNPJ.cs
│   │   │   └── CPF.cs
│   │   ├── Enums/
│   │   │   ├── StatusAssinatura.cs
│   │   │   ├── StatusFatura.cs
│   │   │   └── TipoPlano.cs
│   │   ├── Interfaces/
│   │   │   ├── IAssinanteRepository.cs
│   │   │   ├── IFaturaRepository.cs
│   │   │   ├── IPlanoRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   └── DomainServices/
│   │       ├── CobrancaDomainService.cs
│   │       └── DunningDomainService.cs
│   │
│   ├── Cobrio.Application/               # Use cases & orchestration
│   │   ├── Services/
│   │   │   ├── PlanoService.cs
│   │   │   ├── AssinaturaService.cs
│   │   │   ├── CobrancaService.cs
│   │   │   ├── DunningService.cs
│   │   │   └── DashboardService.cs
│   │   ├── DTOs/
│   │   │   ├── AssinanteDto.cs
│   │   │   ├── PlanoDto.cs
│   │   │   ├── FaturaDto.cs
│   │   │   └── DashboardDto.cs
│   │   ├── Validators/
│   │   │   └── FluentValidation validators
│   │   ├── Mappings/
│   │   │   └── AutoMapper profiles
│   │   └── Interfaces/
│   │       ├── IPaymentGateway.cs
│   │       ├── IEmailService.cs
│   │       └── ISmsService.cs
│   │
│   ├── Cobrio.Infrastructure/            # External concerns
│   │   ├── Data/
│   │   │   ├── CobrioDbContext.cs
│   │   │   ├── Configurations/          # EF Core configs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   │   ├── AssinanteRepository.cs
│   │   │   ├── FaturaRepository.cs
│   │   │   ├── PlanoRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Cache/
│   │   │   ├── RedisCacheService.cs
│   │   │   └── MemoryCacheService.cs
│   │   ├── ExternalServices/
│   │   │   ├── PaymentGateways/
│   │   │   │   ├── StripeGateway.cs
│   │   │   │   └── PagarMeGateway.cs
│   │   │   ├── EmailService.cs          # SendGrid/SMTP
│   │   │   └── SmsService.cs            # Twilio
│   │   └── Jobs/
│   │       ├── GerarFaturasJob.cs
│   │       ├── RetryPagamentoJob.cs
│   │       └── DunningJob.cs
│   │
│   ├── Cobrio.API/                       # Web API
│   │   ├── Controllers/
│   │   │   ├── AssinantesController.cs
│   │   │   ├── PlanosController.cs
│   │   │   ├── FaturasController.cs
│   │   │   ├── DashboardController.cs
│   │   │   └── AuthController.cs
│   │   ├── Middlewares/
│   │   │   ├── MultiTenantMiddleware.cs
│   │   │   ├── ExceptionHandlerMiddleware.cs
│   │   │   └── PerformanceMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── ValidateModelFilter.cs
│   │   │   └── AuthorizationFilter.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs
│   │   └── Program.cs
│   │
│   └── Cobrio.Web/                       # Angular App
│       ├── src/
│       │   ├── app/
│       │   │   ├── core/                 # Singleton services
│       │   │   │   ├── auth/
│       │   │   │   ├── interceptors/
│       │   │   │   └── guards/
│       │   │   ├── shared/               # Shared components
│       │   │   │   ├── components/
│       │   │   │   ├── directives/
│       │   │   │   └── pipes/
│       │   │   ├── features/             # Feature modules
│       │   │   │   ├── dashboard/
│       │   │   │   ├── assinantes/
│       │   │   │   ├── planos/
│       │   │   │   ├── faturas/
│       │   │   │   └── portal-assinante/
│       │   │   └── layout/
│       │   ├── assets/
│       │   └── environments/
│       ├── angular.json
│       └── package.json
│
├── tests/
│   ├── Cobrio.UnitTests/
│   ├── Cobrio.IntegrationTests/
│   └── Cobrio.PerformanceTests/
│
├── docs/
│   ├── arquitetura-tecnica.md
│   ├── modelo-dados.md
│   └── api-documentation.md
│
└── Cobrio.sln
```

---

## 4. Padrões de Design

### 4.1 Repository Pattern
```csharp
public interface IAssinanteRepository : IRepository<Assinante>
{
    Task<Assinante> GetByIdComPlanoAsync(Guid id, Guid empresaId);
    Task<IEnumerable<Assinante>> GetInadimplentesAsync(Guid empresaId);
    Task<PagedResult<Assinante>> GetPagedAsync(int page, int size, Guid empresaId);
}
```

### 4.2 Unit of Work
```csharp
public interface IUnitOfWork : IDisposable
{
    IAssinanteRepository Assinantes { get; }
    IFaturaRepository Faturas { get; }
    IPlanoRepository Planos { get; }

    Task<int> CommitAsync();
    Task RollbackAsync();
}
```

### 4.3 Application Services
```csharp
public class CobrancaService : ICobrancaService
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentGateway _gateway;
    private readonly IEmailService _email;
    private readonly ICacheService _cache;

    public async Task<ResultDto> ProcessarCobrancaAsync(Guid faturaId)
    {
        // Orchestration logic
    }
}
```

---

## 5. Estratégias de Performance

### 5.1 Cache Multi-nível
```
L1: Memory Cache (in-process) → Dados frequentes, TTL curto (5min)
    - Configurações de empresa
    - Planos ativos

L2: Redis Cache (distributed) → Sessões, dados compartilhados
    - Sessões JWT
    - Métricas de dashboard (TTL 30min)
```

### 5.2 Otimização de Queries
- **Select específico**: Evitar `SELECT *`, usar projeções
- **Eager Loading**: `.Include()` para evitar N+1
- **AsNoTracking**: Para queries read-only
- **Compiled Queries**: Para queries repetitivas
- **Índices estratégicos**: Cobertura para queries multi-tenant

```csharp
// Exemplo: Query otimizada
var assinantes = await _context.Assinantes
    .AsNoTracking()
    .Where(a => a.EmpresaClienteId == empresaId && a.Status == StatusAssinatura.Ativo)
    .Select(a => new AssinanteListDto
    {
        Id = a.Id,
        Nome = a.Nome,
        PlanoNome = a.Plano.Nome,
        ProximaCobranca = a.DataFimCiclo
    })
    .ToListAsync();
```

### 5.3 Paginação Eficiente
```csharp
// Cursor-based pagination (melhor performance que offset)
public async Task<PagedResult<T>> GetPagedAsync<T>(
    string cursor,
    int pageSize,
    Guid empresaId)
{
    var query = _context.Set<T>()
        .Where(e => e.EmpresaClienteId == empresaId);

    if (!string.IsNullOrEmpty(cursor))
        query = query.Where(e => e.Id.CompareTo(cursor) > 0);

    var items = await query
        .Take(pageSize + 1)
        .ToListAsync();

    var hasNext = items.Count > pageSize;
    var result = items.Take(pageSize).ToList();
    var nextCursor = hasNext ? result.Last().Id.ToString() : null;

    return new PagedResult<T>(result, nextCursor, hasNext);
}
```

### 5.4 Background Jobs Assíncronos
```csharp
// Usar Hangfire para jobs pesados
BackgroundJob.Enqueue<GerarFaturasJob>(x => x.ExecuteAsync());

// Jobs recorrentes
RecurringJob.AddOrUpdate<RetryPagamentoJob>(
    "retry-pagamentos",
    x => x.ExecuteAsync(),
    Cron.Hourly);
```

### 5.5 Response Compression
```csharp
// Gzip/Brotli compression
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
```

---

## 6. Multi-tenancy

### 6.1 Estratégia: Shared Database + Tenant Isolation via Query Filter

```csharp
public class CobrioDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContext;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Global query filter para todas entidades com EmpresaClienteId
        builder.Entity<Assinante>()
            .HasQueryFilter(a => a.EmpresaClienteId == _currentTenantId);

        builder.Entity<Fatura>()
            .HasQueryFilter(f => f.EmpresaClienteId == _currentTenantId);
    }
}
```

### 6.2 Tenant Context via Middleware
```csharp
public class MultiTenantMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"];
        var tenantId = ExtractTenantFromToken(token);

        context.Items["TenantId"] = tenantId;

        await _next(context);
    }
}
```

---

## 7. Segurança

### 7.1 Autenticação JWT
- **Access Token**: Curta duração (15min), claims: userId, empresaId, roles
- **Refresh Token**: Longa duração (7 dias), armazenado em cookie HttpOnly

### 7.2 Autorização
```csharp
[Authorize(Roles = "Admin,Operador")]
[MultiTenantAuthorize] // Custom filter
public class AssinantesController : ControllerBase
```

### 7.3 Data Protection
- **Criptografia em repouso**: Campos sensíveis (método pagamento) via EF Core Value Converter
- **Tokenização**: Dados de cartão NUNCA armazenados, apenas tokens do gateway
- **HTTPS obrigatório**: Redirect HTTP → HTTPS

---

## 8. Observabilidade

### 8.1 Logging Estruturado (Serilog)
```csharp
Log.Information("Cobrança processada {@Fatura} {@Resultado}",
    fatura, resultado);
```

### 8.2 Health Checks
```
/health          → Status geral (API, DB, Redis, Gateway)
/health/ready    → Readiness probe
/health/live     → Liveness probe
```

### 8.3 Métricas (Application Insights / Prometheus)
- Tempo de resposta API
- Taxa de falha de cobrança
- Jobs executados/falhados
- Cache hit rate

---

## 9. Tecnologias e Bibliotecas

### Backend (.NET 7+)
- **ORM**: Entity Framework Core 7+
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Cache**: StackExchange.Redis + IMemoryCache
- **Jobs**: Hangfire
- **Logging**: Serilog
- **Auth**: Microsoft.AspNetCore.Authentication.JwtBearer
- **Testing**: xUnit + Moq + FluentAssertions

### Frontend (Angular 16+)
- **UI Components**: PrimeNG + Tailwind CSS
- **State Management**: RxJS + Services (simples)
- **Forms**: Reactive Forms
- **HTTP**: HttpClient + Interceptors
- **Charts**: Chart.js / ApexCharts
- **Auth**: @auth0/angular-jwt
- **Testing**: Jasmine + Karma

### Database
- **MySQL 8+**: InnoDB, utf8mb4
- **Migrations**: EF Core Migrations
- **Backup**: Automatizado diário

---

## 10. Deploy e Infraestrutura

### 10.1 Sem Docker (VMs/App Services)

#### Backend
- **Hosting**: IIS ou Azure App Service
- **Build**: `dotnet publish -c Release`
- **Deploy**: CI/CD via GitHub Actions ou Azure DevOps

#### Frontend
- **Build**: `ng build --configuration production`
- **Output**: `dist/` → IIS/Nginx/Apache
- **CDN**: Cloudflare para assets estáticos

#### Database
- **Opção 1**: MySQL em VM dedicada
- **Opção 2**: Azure Database for MySQL (managed)

### 10.2 Ambientes
```
Development  → localhost
Staging      → staging.cobrio.com.br
Production   → app.cobrio.com.br
```

---

## 11. Próximos Passos

1. ✅ Criar estrutura de projetos
2. ✅ Configurar solution C#
3. ✅ Modelar banco de dados MySQL
4. ✅ Implementar camada Domain (Entities)
5. ✅ Implementar Infrastructure (EF Core + Repositories)
6. ✅ Criar API base com autenticação
7. ✅ Configurar projeto Angular
8. ✅ Implementar MVP (módulos core)

---

**Versão**: 1.0
**Data**: 2025-10-26
**Status**: Em Desenvolvimento

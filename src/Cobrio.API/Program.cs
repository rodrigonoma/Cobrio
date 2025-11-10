using Cobrio.Application.Interfaces;
using Cobrio.Application.Interfaces.Notifications;
using Cobrio.Application.Jobs;
using Cobrio.Application.Services;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Cobrio.Infrastructure.Notifications;
using Cobrio.Infrastructure.Repositories;
using Cobrio.API.Middleware;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using OfficeOpenXml;
using System.Globalization;

// Configurar cultura padrão da aplicação (pt-BR)
var cultureInfo = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Configurar licença do EPPlus 8
ExcelPackage.License.SetNonCommercialPersonal("Cobrio");

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cobrio-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Database
builder.Services.AddDbContext<CobrioDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
    {
        mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Repositories & Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmpresaClienteRepository, EmpresaClienteRepository>();
builder.Services.AddScoped<IAssinanteRepository, AssinanteRepository>();
builder.Services.AddScoped<IPlanoOfertaRepository, PlanoOfertaRepository>();
builder.Services.AddScoped<IFaturaRepository, FaturaRepository>();
builder.Services.AddScoped<IUsuarioEmpresaRepository, UsuarioEmpresaRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRegraCobrancaRepository, RegraCobrancaRepository>();
builder.Services.AddScoped<ICobrancaRepository, CobrancaRepository>();
builder.Services.AddScoped<IHistoricoNotificacaoRepository, HistoricoNotificacaoRepository>();
builder.Services.AddScoped<IHistoricoImportacaoRepository, HistoricoImportacaoRepository>();
builder.Services.AddScoped<IPermissaoRepository, PermissaoRepository>();
builder.Services.AddScoped<ITemplateEmailRepository, TemplateEmailRepository>();

// Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlanoOfertaService, PlanoOfertaService>();
builder.Services.AddScoped<IAssinaturaService, AssinaturaService>();
builder.Services.AddScoped<IRegraCobrancaService, RegraCobrancaService>();
builder.Services.AddScoped<IPermissaoService, PermissaoService>();
builder.Services.AddScoped<CobrancaService>();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<UsuarioEmpresaService>();

// Reports Services
builder.Services.AddHttpClient<BrevoEmailStatsService>();
builder.Services.AddScoped<Cobrio.API.Services.RelatoriosService>();
builder.Services.AddScoped<Cobrio.API.Services.RelatoriosAvancadosService>();
builder.Services.AddScoped<IAnalyticsService, Cobrio.API.Services.AnalyticsService>();
builder.Services.AddScoped<Cobrio.API.Services.TemplateEmailService>();

// Memory Cache
builder.Services.AddMemoryCache();

// Notification Settings
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.Configure<Cobrio.Application.Configuration.BrevoSettings>(builder.Configuration.GetSection("Brevo"));

// Notification Providers
builder.Services.AddScoped<IEmailProvider, BrevoEmailProvider>();
builder.Services.AddScoped<ISmsProvider, TwilioSmsProvider>();
builder.Services.AddScoped<IWhatsAppProvider, TwilioWhatsAppProvider>();

// Email Service (Brevo)
builder.Services.AddHttpClient<Cobrio.Application.Interfaces.IEmailService, Cobrio.Application.Services.BrevoEmailService>();

// Notification Factory & Service
builder.Services.AddScoped<INotificationChannelFactory, NotificationChannelFactory>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Background Jobs
builder.Services.AddScoped<ProcessarCobrancasJob>();

// HttpContextAccessor para multi-tenant
builder.Services.AddHttpContextAccessor();

// Hangfire - usando banco separado (recomendado)
var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection");

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(new MySqlStorage(
        hangfireConnection,
        new MySqlStorageOptions
        {
            QueuePollInterval = TimeSpan.FromSeconds(15),
            JobExpirationCheckInterval = TimeSpan.FromHours(1),
            CountersAggregateInterval = TimeSpan.FromMinutes(5),
            PrepareSchemaIfNecessary = true, // cria tabelas automaticamente
            DashboardJobListLimit = 50000,
            TransactionTimeout = TimeSpan.FromMinutes(1),
            TablesPrefix = "Hangfire"
        }
    )));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 5;
    options.ServerName = $"Cobrio-{Environment.MachineName}";
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI com suporte a Bearer Token
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Cobrio API",
        Version = "v1",
        Description = "API para automação de cobrança recorrente com multi-tenancy",
        Contact = new OpenApiContact
        {
            Name = "Cobrio",
            Email = "contato@cobrio.com.br"
        }
    });

    // Configuração para Bearer Token
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Executar seed em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CobrioDbContext>();
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        // Seed original
        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();

        // Seed de permissões
        var permissaoSeeder = new PermissaoSeeder(context);
        await permissaoSeeder.SeedModulosEAcoesAsync();

        // Pegar o proprietário criado pelo seed original
        var proprietario = context.UsuariosEmpresa.FirstOrDefault(u => u.EhProprietario);
        if (proprietario != null)
        {
            await permissaoSeeder.SeedPermissoesDefaultAsync(proprietario.EmpresaClienteId, proprietario.Id);
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao executar seed do banco de dados");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cobrio API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseSerilogRequestLogging();

// CORS deve vir antes de outros middlewares que podem redirecionar
app.UseCors("AllowAll");

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "Cobrio - Background Jobs"
});

// HTTPS redirection apenas em produção para evitar problemas com CORS em desenvolvimento
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// IMPORTANTE: ordem correta dos middlewares
app.UseAuthentication();  // Primeiro autenticação
app.UseTenantMiddleware(); // Depois extrair tenant do token
app.UseAuthorization();    // Por último autorização

app.MapControllers();

// Configurar job recorrente
RecurringJob.AddOrUpdate<ProcessarCobrancasJob>(
    "processar-cobrancas",
    job => job.ExecutarAsync(CancellationToken.None),
    Cron.Minutely); // Executa a cada minuto

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();

// Database health check
app.MapGet("/health/database", async (CobrioDbContext dbContext) =>
{
    try
    {
        await dbContext.Database.CanConnectAsync();
        return Results.Ok(new { status = "healthy", database = "connected" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Database unhealthy");
    }
}).AllowAnonymous();

try
{
    Log.Information("Starting Cobrio API on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

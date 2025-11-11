using Cobrio.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Cobrio.Infrastructure.Data;

public class CobrioDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CobrioDbContext(DbContextOptions<CobrioDbContext> options)
        : base(options)
    {
    }

    public CobrioDbContext(
        DbContextOptions<CobrioDbContext> options,
        IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // DbSets
    public DbSet<EmpresaCliente> EmpresasCliente { get; set; } = null!;
    public DbSet<UsuarioEmpresa> UsuariosEmpresa { get; set; } = null!;
    public DbSet<PlanoOferta> PlanosOferta { get; set; } = null!;
    public DbSet<Assinante> Assinantes { get; set; } = null!;
    public DbSet<MetodoPagamento> MetodosPagamento { get; set; } = null!;
    public DbSet<Fatura> Faturas { get; set; } = null!;
    public DbSet<ItemFatura> ItensFatura { get; set; } = null!;
    public DbSet<TentativaPagamento> TentativasPagamento { get; set; } = null!;
    public DbSet<ReguaDunningConfig> ReguasDunning { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<RegraCobranca> RegrasCobranca { get; set; } = null!;
    public DbSet<Cobranca> Cobrancas { get; set; } = null!;
    public DbSet<HistoricoNotificacao> HistoricosNotificacao { get; set; } = null!;
    public DbSet<HistoricoStatusNotificacao> HistoricosStatusNotificacao { get; set; } = null!;
    public DbSet<HistoricoImportacao> HistoricosImportacao { get; set; } = null!;

    // Permissões
    public DbSet<Modulo> Modulos { get; set; } = null!;
    public DbSet<Acao> Acoes { get; set; } = null!;
    public DbSet<PermissaoPerfil> PermissoesPerfil { get; set; } = null!;

    // Templates
    public DbSet<TemplateEmail> TemplatesEmail { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas as configurations do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Garantir nomenclatura consistente de tabelas em PascalCase em todos os ambientes
        // Isso evita problemas com diferenças entre MySQL no Windows (case-insensitive) e Linux (case-sensitive)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Ignorar owned entities (como Endereco que é owned por EmpresaCliente e Assinante)
            if (entity.IsOwned())
                continue;

            var tableName = entity.GetTableName();

            // Se a tabela não tem nome configurado explicitamente, usa o nome da classe da entidade
            if (string.IsNullOrEmpty(tableName) || tableName != entity.ClrType.Name)
            {
                entity.SetTableName(entity.ClrType.Name);
            }
        }

        // Aplicar Global Query Filters para Multi-tenant
        ConfigureMultiTenantFilters(modelBuilder);
    }

    private void ConfigureMultiTenantFilters(ModelBuilder modelBuilder)
    {
        // Obter TenantId do contexto HTTP (será configurado no middleware)
        var tenantId = GetCurrentTenantId();

        // Aplicar filtro global em todas as entidades que têm EmpresaClienteId
        modelBuilder.Entity<PlanoOferta>()
            .HasQueryFilter(p => tenantId == null || p.EmpresaClienteId == tenantId);

        modelBuilder.Entity<Assinante>()
            .HasQueryFilter(a => tenantId == null || a.EmpresaClienteId == tenantId);

        modelBuilder.Entity<MetodoPagamento>()
            .HasQueryFilter(m => tenantId == null || m.EmpresaClienteId == tenantId);

        modelBuilder.Entity<Fatura>()
            .HasQueryFilter(f => tenantId == null || f.EmpresaClienteId == tenantId);

        modelBuilder.Entity<TentativaPagamento>()
            .HasQueryFilter(t => tenantId == null || t.EmpresaClienteId == tenantId);

        modelBuilder.Entity<UsuarioEmpresa>()
            .HasQueryFilter(u => tenantId == null || u.EmpresaClienteId == tenantId);

        modelBuilder.Entity<ReguaDunningConfig>()
            .HasQueryFilter(r => tenantId == null || r.EmpresaClienteId == tenantId);

        modelBuilder.Entity<RegraCobranca>()
            .HasQueryFilter(r => tenantId == null || r.EmpresaClienteId == tenantId);

        modelBuilder.Entity<Cobranca>()
            .HasQueryFilter(c => tenantId == null || c.EmpresaClienteId == tenantId);

        modelBuilder.Entity<HistoricoNotificacao>()
            .HasQueryFilter(h => tenantId == null || h.EmpresaClienteId == tenantId);

        modelBuilder.Entity<PermissaoPerfil>()
            .HasQueryFilter(p => tenantId == null || p.EmpresaClienteId == tenantId);
    }

    private Guid? GetCurrentTenantId()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return null;

        var tenantIdClaim = _httpContextAccessor.HttpContext.Items["TenantId"]?.ToString();

        if (string.IsNullOrEmpty(tenantIdClaim))
            return null;

        return Guid.TryParse(tenantIdClaim, out var tenantId) ? tenantId : null;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Atualizar campos de auditoria automaticamente
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.AtualizarDataModificacao();
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    // Método para desabilitar filtros (útil para operações admin)
    public IQueryable<T> SetIgnoreQueryFilters<T>() where T : class
    {
        return Set<T>().IgnoreQueryFilters();
    }
}

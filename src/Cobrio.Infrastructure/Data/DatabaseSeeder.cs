using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cobrio.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly CobrioDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(CobrioDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Verifica se já existe alguma empresa no banco
            if (await _context.EmpresasCliente.AnyAsync())
            {
                _logger.LogInformation("Banco de dados já possui dados. Seed não executado.");
                return;
            }

            _logger.LogInformation("Iniciando seed do banco de dados...");

            // Criar endereço
            var endereco = new Endereco(
                logradouro: "Av. Paulista",
                numero: "1000",
                bairro: "Bela Vista",
                cidade: "São Paulo",
                estado: "SP",
                cep: "01310100",
                pais: "Brasil"
            );

            // Criar Empresa Cliente de exemplo
            var empresaCliente = new EmpresaCliente(
                nome: "Empresa Demo Ltda",
                cnpj: new CNPJ("11222333000181"),
                email: new Email("contato@empresademo.com.br"),
                planoCobrioId: 1,
                telefone: "11987654321",
                endereco: endereco
            );

            await _context.EmpresasCliente.AddAsync(empresaCliente);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Empresa cliente criada: {Nome}", empresaCliente.Nome);

            // Criar Usuário Admin para a empresa (primeiro usuário é o proprietário)
            var senhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            var usuarioAdmin = new UsuarioEmpresa(
                empresaClienteId: empresaCliente.Id,
                nome: "Administrador",
                email: new Email("admin@empresademo.com.br"),
                passwordHash: senhaHash,
                perfil: PerfilUsuario.Admin,
                ehProprietario: true
            );

            await _context.UsuariosEmpresa.AddAsync(usuarioAdmin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário admin proprietário criado: {Email}", usuarioAdmin.Email.Endereco);

            // Criar Plano de Oferta de exemplo
            var planoBasico = new PlanoOferta(
                empresaClienteId: empresaCliente.Id,
                nome: "Plano Básico",
                tipoCiclo: TipoCiclo.Mensal,
                valor: Money.FromDecimal(49.90m),
                descricao: "Plano básico para pequenas empresas",
                periodoTrialDias: 7,
                limiteUsuarios: 5
            );

            await _context.PlanosOferta.AddAsync(planoBasico);

            var planoAvancado = new PlanoOferta(
                empresaClienteId: empresaCliente.Id,
                nome: "Plano Avançado",
                tipoCiclo: TipoCiclo.Mensal,
                valor: Money.FromDecimal(99.90m),
                descricao: "Plano avançado para médias empresas",
                periodoTrialDias: 14,
                limiteUsuarios: 20
            );

            await _context.PlanosOferta.AddAsync(planoAvancado);

            var planoAnual = new PlanoOferta(
                empresaClienteId: empresaCliente.Id,
                nome: "Plano Anual",
                tipoCiclo: TipoCiclo.Anual,
                valor: Money.FromDecimal(999.00m),
                descricao: "Plano anual com desconto",
                periodoTrialDias: 30,
                limiteUsuarios: null // Ilimitado
            );

            await _context.PlanosOferta.AddAsync(planoAnual);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Planos de oferta criados: 3 planos");

            // Criar Régua de Dunning padrão
            var reguaDunning = new ReguaDunningConfig(
                empresaClienteId: empresaCliente.Id,
                numeroMaximoTentativas: 3,
                intervalosDias: new List<int> { 1, 3, 7 },
                enviarEmail: true,
                enviarSMS: false,
                enviarNotificacaoInApp: true,
                diasSuspensao: 15,
                diasCancelamento: 30,
                horaInicioRetry: new TimeSpan(8, 0, 0),
                horaFimRetry: new TimeSpan(20, 0, 0)
            );

            await _context.ReguasDunning.AddAsync(reguaDunning);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Régua de dunning configurada");

            // Criar Regra de Cobrança Padrão (Envio Imediato)
            var regraCobrancaPadrao = RegraCobranca.CriarRegraPadrao(empresaCliente.Id, Domain.Enums.CanalNotificacao.Email);
            await _context.RegrasCobranca.AddAsync(regraCobrancaPadrao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Regra de cobrança padrão criada: {Nome}", regraCobrancaPadrao.Nome);

            _logger.LogInformation("Seed concluído com sucesso!");
            _logger.LogInformation("==================================================");
            _logger.LogInformation("Credenciais de acesso:");
            _logger.LogInformation("Email: admin@empresademo.com.br");
            _logger.LogInformation("Senha: Admin@123");
            _logger.LogInformation("Empresa ID: {EmpresaId}", empresaCliente.Id);
            _logger.LogInformation("==================================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar seed do banco de dados");
            throw;
        }
    }
}

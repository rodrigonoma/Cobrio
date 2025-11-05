# Modelo de Dados - Cobrio

## 1. Visão Geral

Banco de dados: **MySQL 8.0+**
Charset: **utf8mb4** (suporte completo Unicode, incluindo emojis)
Collation: **utf8mb4_unicode_ci**
Engine: **InnoDB** (suporte a transações, foreign keys, performance)

### Estratégia Multi-tenant
- **Shared Database + Tenant Isolation**: Todas as tabelas de dados de negócio contêm `EmpresaClienteId`
- **Query Filters**: EF Core Global Query Filters para isolamento automático
- **Índices compostos**: Sempre começam com `EmpresaClienteId` para performance

---

## 2. Diagrama de Relacionamentos (Resumo)

```
EmpresaCliente (1) ──────┬──────── (N) UsuarioEmpresa
                         │
                         ├──────── (N) PlanoOferta
                         │              │
                         │              └──── (N) Assinante
                         │                       │
                         ├──────────────────────┤
                         │                       │
                         │                       ├──── (N) MetodoPagamento
                         │                       │
                         │                       └──── (N) Fatura
                         │                                │
                         │                                ├──── (N) TentativaPagamento
                         │                                │
                         │                                └──── (N) ItemFatura
                         │
                         ├──────── (1) ReguaDunningConfig
                         │
                         ├──────── (N) TemplateComunicacao
                         │
                         └──────── (N) LogComunicacao
```

---

## 3. Tabelas

### 3.1 EmpresaCliente
Empresa que usa a plataforma Cobrio (cliente da plataforma).

```sql
CREATE TABLE EmpresaCliente (
    Id CHAR(36) PRIMARY KEY,
    Nome VARCHAR(200) NOT NULL,
    CNPJ CHAR(14) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL,
    Telefone VARCHAR(20),

    -- Plano Cobrio
    PlanoCobrioId INT NOT NULL,
    DataContrato DATETIME NOT NULL,
    StatusContrato ENUM('Ativo', 'Suspenso', 'Cancelado') NOT NULL DEFAULT 'Ativo',

    -- Endereço
    Endereco_Logradouro VARCHAR(200),
    Endereco_Numero VARCHAR(20),
    Endereco_Complemento VARCHAR(100),
    Endereco_Bairro VARCHAR(100),
    Endereco_Cidade VARCHAR(100),
    Endereco_Estado CHAR(2),
    Endereco_CEP CHAR(8),
    Endereco_Pais VARCHAR(50) DEFAULT 'Brasil',

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    INDEX idx_cnpj (CNPJ),
    INDEX idx_status (StatusContrato),
    INDEX idx_plano (PlanoCobrioId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.2 UsuarioEmpresa
Usuários (funcionários) da empresa cliente que acessam o painel Cobrio.

```sql
CREATE TABLE UsuarioEmpresa (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,

    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,

    Perfil ENUM('Admin', 'Operador', 'Visualizador') NOT NULL DEFAULT 'Operador',
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,

    UltimoAcesso DATETIME,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_email_empresa (Email, EmpresaClienteId),
    INDEX idx_tenant_email (EmpresaClienteId, Email),
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.3 PlanoOferta
Planos/produtos que a empresa cliente oferece aos seus assinantes.

```sql
CREATE TABLE PlanoOferta (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,

    Nome VARCHAR(100) NOT NULL,
    Descricao TEXT,

    -- Pricing
    TipoCiclo ENUM('Mensal', 'Trimestral', 'Semestral', 'Anual', 'Uso') NOT NULL DEFAULT 'Mensal',
    ValorCentavos BIGINT NOT NULL, -- Valor em centavos para evitar problemas com float
    Moeda CHAR(3) NOT NULL DEFAULT 'BRL',

    -- Configurações
    PeriodoTrial INT DEFAULT 0, -- Dias de trial
    LimiteUsuarios INT, -- NULL = ilimitado
    PermiteDowngrade BOOLEAN NOT NULL DEFAULT TRUE,
    PermiteUpgrade BOOLEAN NOT NULL DEFAULT TRUE,

    Ativo BOOLEAN NOT NULL DEFAULT TRUE,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo),
    INDEX idx_tenant_nome (EmpresaClienteId, Nome)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.4 Assinante
Usuários finais que assinam os planos da empresa cliente.

```sql
CREATE TABLE Assinante (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    PlanoOfertaId CHAR(36) NOT NULL,

    -- Dados pessoais
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    CPFCNPJ VARCHAR(14),
    Telefone VARCHAR(20),

    -- Assinatura
    Status ENUM('Ativo', 'AguardandoPagamento', 'Trial', 'Inadimplente', 'Suspenso', 'Cancelado') NOT NULL DEFAULT 'Ativo',
    DataInicio DATETIME NOT NULL,
    DataFimCiclo DATETIME NOT NULL,
    DataCancelamento DATETIME,
    MotivoCancelamento TEXT,

    -- Trial
    EmTrial BOOLEAN NOT NULL DEFAULT FALSE,
    DataFimTrial DATETIME,

    -- Cobrança
    DiaVencimento INT NOT NULL, -- 1-31
    ProximaCobranca DATETIME NOT NULL,

    -- Endereço
    Endereco_Logradouro VARCHAR(200),
    Endereco_Numero VARCHAR(20),
    Endereco_Complemento VARCHAR(100),
    Endereco_Bairro VARCHAR(100),
    Endereco_Cidade VARCHAR(100),
    Endereco_Estado CHAR(2),
    Endereco_CEP CHAR(8),

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlanoOfertaId) REFERENCES PlanoOferta(Id) ON DELETE RESTRICT,

    INDEX idx_tenant_status (EmpresaClienteId, Status),
    INDEX idx_tenant_email (EmpresaClienteId, Email),
    INDEX idx_tenant_proxima_cobranca (EmpresaClienteId, ProximaCobranca),
    INDEX idx_proxima_cobranca (ProximaCobranca), -- Para jobs que processam cobranças
    INDEX idx_plano (PlanoOfertaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.5 MetodoPagamento
Métodos de pagamento dos assinantes (tokenizados).

```sql
CREATE TABLE MetodoPagamento (
    Id CHAR(36) PRIMARY KEY,
    AssinanteId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,

    Tipo ENUM('Cartao', 'Boleto', 'Pix', 'DebitoAutomatico') NOT NULL,

    -- Tokenizado (NUNCA armazenar dados completos de cartão)
    TokenGateway VARCHAR(255), -- Token do gateway de pagamento
    GatewayProvider VARCHAR(50), -- 'Stripe', 'PagarMe', etc

    -- Dados seguros para exibição
    UltimosDigitos VARCHAR(4),
    Bandeira VARCHAR(30), -- Visa, Mastercard, etc
    NomeTitular VARCHAR(150),

    DataValidade DATE,
    Principal BOOLEAN NOT NULL DEFAULT FALSE, -- Método principal

    Ativo BOOLEAN NOT NULL DEFAULT TRUE,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,

    INDEX idx_assinante (AssinanteId),
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo),
    INDEX idx_assinante_principal (AssinanteId, Principal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.6 Fatura
Faturas/invoices geradas para os assinantes.

```sql
CREATE TABLE Fatura (
    Id CHAR(36) PRIMARY KEY,
    AssinanteId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,

    NumeroFatura VARCHAR(50) NOT NULL, -- Número sequencial único por empresa

    -- Valores (em centavos)
    ValorBrutoCentavos BIGINT NOT NULL,
    DescontoCentavos BIGINT NOT NULL DEFAULT 0,
    ImpostosCentavos BIGINT NOT NULL DEFAULT 0,
    ValorLiquidoCentavos BIGINT NOT NULL,
    Moeda CHAR(3) NOT NULL DEFAULT 'BRL',

    -- Datas
    DataEmissao DATETIME NOT NULL,
    DataVencimento DATETIME NOT NULL,
    DataPagamento DATETIME,

    -- Status
    Status ENUM('Pendente', 'AguardandoPagamento', 'Pago', 'Falhou', 'Cancelado', 'Reembolsado') NOT NULL DEFAULT 'Pendente',

    -- Referências
    MetodoPagamentoId CHAR(36),
    TransacaoIdGateway VARCHAR(255), -- ID da transação no gateway

    -- Observações
    Observacoes TEXT,
    LinkBoleto TEXT, -- Se for boleto
    QrCodePix TEXT, -- Se for Pix

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (MetodoPagamentoId) REFERENCES MetodoPagamento(Id) ON DELETE SET NULL,

    UNIQUE KEY uk_numero_fatura (EmpresaClienteId, NumeroFatura),
    INDEX idx_tenant_status (EmpresaClienteId, Status),
    INDEX idx_tenant_vencimento (EmpresaClienteId, DataVencimento),
    INDEX idx_assinante_status (AssinanteId, Status),
    INDEX idx_data_vencimento (DataVencimento), -- Para jobs de cobrança
    INDEX idx_status_vencimento (Status, DataVencimento)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
PARTITION BY RANGE (YEAR(DataEmissao)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);
```

### 3.7 ItemFatura
Itens/linhas da fatura (plano principal + add-ons).

```sql
CREATE TABLE ItemFatura (
    Id CHAR(36) PRIMARY KEY,
    FaturaId CHAR(36) NOT NULL,

    Descricao VARCHAR(255) NOT NULL,
    Quantidade INT NOT NULL DEFAULT 1,
    ValorUnitarioCentavos BIGINT NOT NULL,
    ValorTotalCentavos BIGINT NOT NULL,

    TipoItem ENUM('Plano', 'AddOn', 'Ajuste', 'Desconto', 'Taxa') NOT NULL DEFAULT 'Plano',

    -- Referência
    PlanoOfertaId CHAR(36),

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (FaturaId) REFERENCES Fatura(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlanoOfertaId) REFERENCES PlanoOferta(Id) ON DELETE SET NULL,

    INDEX idx_fatura (FaturaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.8 TentativaPagamento
Tentativas de pagamento (para dunning/retry).

```sql
CREATE TABLE TentativaPagamento (
    Id CHAR(36) PRIMARY KEY,
    FaturaId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,

    NumeroTentativa INT NOT NULL, -- 1, 2, 3...
    DataTentativa DATETIME NOT NULL,

    Resultado ENUM('Sucesso', 'Falha', 'Processando', 'Cancelado') NOT NULL,
    CodigoErro VARCHAR(50),
    MensagemErro TEXT,

    -- Gateway
    TransacaoIdGateway VARCHAR(255),
    GatewayProvider VARCHAR(50),

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (FaturaId) REFERENCES Fatura(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,

    INDEX idx_fatura (FaturaId),
    INDEX idx_tenant_data (EmpresaClienteId, DataTentativa),
    INDEX idx_resultado (Resultado)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
PARTITION BY RANGE (YEAR(DataTentativa)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);
```

### 3.9 ReguaDunningConfig
Configuração da régua de cobrança por empresa.

```sql
CREATE TABLE ReguaDunningConfig (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL UNIQUE,

    -- Tentativas
    NumeroMaximoTentativas INT NOT NULL DEFAULT 3,
    IntervalosDiasJson JSON NOT NULL, -- [1, 3, 7] - dias entre tentativas

    -- Métodos de contato
    EnviarEmail BOOLEAN NOT NULL DEFAULT TRUE,
    EnviarSMS BOOLEAN NOT NULL DEFAULT FALSE,
    EnviarNotificacaoInApp BOOLEAN NOT NULL DEFAULT TRUE,

    -- Ações automáticas
    DiasSuspensao INT NOT NULL DEFAULT 15, -- Suspender após X dias
    DiasCancelamento INT NOT NULL DEFAULT 30, -- Cancelar após X dias

    -- Horários permitidos para retry
    HoraInicioRetry TIME DEFAULT '08:00:00',
    HoraFimRetry TIME DEFAULT '20:00:00',

    Ativo BOOLEAN NOT NULL DEFAULT TRUE,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.10 TemplateComunicacao
Templates de email/SMS para comunicações.

```sql
CREATE TABLE TemplateComunicacao (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,

    Nome VARCHAR(100) NOT NULL,
    Tipo ENUM('PreVencimento', 'Vencimento', 'FalhaPagamento', 'AvisoSuspensao', 'AvisoCancelamento', 'Boas-Vindas', 'Confirmacao') NOT NULL,
    Canal ENUM('Email', 'SMS', 'InApp') NOT NULL,

    -- Conteúdo
    Assunto VARCHAR(200), -- Para email
    CorpoTexto TEXT NOT NULL, -- Suporta variáveis: {{nome}}, {{valor}}, etc
    CorpoHtml TEXT, -- Para email

    Ativo BOOLEAN NOT NULL DEFAULT TRUE,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_tenant_tipo (EmpresaClienteId, Tipo, Canal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 3.11 LogComunicacao
Log de todas comunicações enviadas (auditoria + analytics).

```sql
CREATE TABLE LogComunicacao (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    AssinanteId CHAR(36) NOT NULL,
    FaturaId CHAR(36),

    TipoContato ENUM('Email', 'SMS', 'InApp') NOT NULL,
    TemplateId CHAR(36),

    -- Destinatário
    Destinatario VARCHAR(200) NOT NULL, -- Email ou telefone
    Assunto VARCHAR(200),

    -- Status envio
    DataEnvio DATETIME NOT NULL,
    StatusEnvio ENUM('Enviado', 'Falhou', 'Bounce', 'Spam') NOT NULL DEFAULT 'Enviado',
    MensagemErro TEXT,

    -- Tracking (email)
    DataAbertura DATETIME,
    DataClique DATETIME,
    NumeroAberturas INT DEFAULT 0,
    NumeroCliques INT DEFAULT 0,

    -- Provider
    ProviderId VARCHAR(255), -- ID do provedor (SendGrid, Twilio, etc)

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
    FOREIGN KEY (FaturaId) REFERENCES Fatura(Id) ON DELETE SET NULL,
    FOREIGN KEY (TemplateId) REFERENCES TemplateComunicacao(Id) ON DELETE SET NULL,

    INDEX idx_tenant_data (EmpresaClienteId, DataEnvio),
    INDEX idx_assinante_data (AssinanteId, DataEnvio),
    INDEX idx_fatura (FaturaId),
    INDEX idx_tipo_status (TipoContato, StatusEnvio)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
PARTITION BY RANGE (YEAR(DataEnvio)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);
```

### 3.12 WebhookLog
Log de webhooks enviados para clientes.

```sql
CREATE TABLE WebhookLog (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,

    Evento VARCHAR(100) NOT NULL, -- 'pagamento.sucesso', 'pagamento.falha', etc
    URL VARCHAR(500) NOT NULL,

    PayloadJson JSON NOT NULL,

    StatusHTTP INT,
    RespostaBody TEXT,

    NumeroTentativas INT NOT NULL DEFAULT 1,
    DataEnvio DATETIME NOT NULL,
    DataProximaTentativa DATETIME,

    Sucesso BOOLEAN NOT NULL DEFAULT FALSE,

    -- Auditoria
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,

    INDEX idx_tenant_evento (EmpresaClienteId, Evento),
    INDEX idx_sucesso_proxima (Sucesso, DataProximaTentativa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

## 4. Views Materializadas (Para Dashboards)

### 4.1 View de Métricas Diárias

```sql
CREATE VIEW vw_MetricasDiarias AS
SELECT
    e.Id AS EmpresaClienteId,
    DATE(f.DataEmissao) AS Data,
    COUNT(DISTINCT a.Id) AS TotalAssinantesAtivos,
    COUNT(DISTINCT CASE WHEN f.Status = 'Pago' THEN f.Id END) AS FaturasPagas,
    COUNT(DISTINCT CASE WHEN f.Status = 'Falhou' THEN f.Id END) AS FaturasFalhadas,
    SUM(CASE WHEN f.Status = 'Pago' THEN f.ValorLiquidoCentavos ELSE 0 END) / 100.0 AS ReceitaDia,
    COUNT(DISTINCT CASE WHEN a.Status = 'Cancelado' THEN a.Id END) AS ChurnDia
FROM EmpresaCliente e
LEFT JOIN Assinante a ON e.Id = a.EmpresaClienteId AND a.Status = 'Ativo'
LEFT JOIN Fatura f ON e.Id = f.EmpresaClienteId
GROUP BY e.Id, DATE(f.DataEmissao);
```

---

## 5. Triggers para Auditoria Automática

### 5.1 Trigger para log de alterações em Assinante

```sql
DELIMITER $$

CREATE TRIGGER trg_Assinante_AuditLog
AFTER UPDATE ON Assinante
FOR EACH ROW
BEGIN
    IF OLD.Status <> NEW.Status THEN
        INSERT INTO AuditoriaLog (
            EntidadeId,
            TipoEntidade,
            EmpresaClienteId,
            Acao,
            ValorAntigo,
            ValorNovo,
            DataAcao
        ) VALUES (
            NEW.Id,
            'Assinante',
            NEW.EmpresaClienteId,
            'MudancaStatus',
            OLD.Status,
            NEW.Status,
            NOW()
        );
    END IF;
END$$

DELIMITER ;
```

---

## 6. Índices de Performance

### Índices críticos para queries multi-tenant:

1. **Todas as queries começam com `EmpresaClienteId`** → Índices compostos sempre com tenant primeiro
2. **Particionamento** em tabelas grandes (Fatura, TentativaPagamento, LogComunicacao) por ano
3. **Índices covering** para queries frequentes de dashboard

### Estratégia de Particionamento:
- Tabelas com crescimento contínuo: particionamento por YEAR()
- Facilita manutenção (drop de partições antigas)
- Melhora performance de queries com filtros de data

---

## 7. Stored Procedures para Performance

### 7.1 Calcular MRR (Monthly Recurring Revenue)

```sql
DELIMITER $$

CREATE PROCEDURE sp_CalcularMRR(IN p_EmpresaClienteId CHAR(36))
BEGIN
    SELECT
        SUM(po.ValorCentavos) / 100.0 AS MRR
    FROM Assinante a
    INNER JOIN PlanoOferta po ON a.PlanoOfertaId = po.Id
    WHERE a.EmpresaClienteId = p_EmpresaClienteId
      AND a.Status = 'Ativo'
      AND po.TipoCiclo = 'Mensal';
END$$

DELIMITER ;
```

---

## 8. Otimizações Recomendadas

### 8.1 Cache Strategy
- **Planos ativos**: Cache in-memory (5 min TTL)
- **Config dunning**: Cache in-memory (15 min TTL)
- **Métricas dashboard**: Redis (30 min TTL)

### 8.2 Queries Compiladas (EF Core)
```csharp
private static readonly Func<CobrioDbContext, Guid, Task<List<Assinante>>> _getAssinantesAtivos =
    EF.CompileAsyncQuery((CobrioDbContext ctx, Guid empresaId) =>
        ctx.Assinantes
            .Where(a => a.EmpresaClienteId == empresaId && a.Status == StatusAssinatura.Ativo)
            .Include(a => a.Plano)
            .ToList());
```

### 8.3 Connection Pooling
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=cobrio_db;User=cobrio_user;Password=***;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connection Lifetime=0;"
}
```

---

## 9. Scripts de Manutenção

### 9.1 Limpeza de logs antigos (executar mensalmente)

```sql
-- Deletar logs de comunicação > 2 anos
DELETE FROM LogComunicacao
WHERE DataEnvio < DATE_SUB(NOW(), INTERVAL 2 YEAR);

-- Deletar tentativas de pagamento > 2 anos
DELETE FROM TentativaPagamento
WHERE DataTentativa < DATE_SUB(NOW(), INTERVAL 2 YEAR);
```

---

**Versão**: 1.0
**Data**: 2025-10-26
**Próximas atualizações**: Add-ons, Cupons, Multi-moeda avançada

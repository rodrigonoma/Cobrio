-- ============================================
-- Cobrio Database Schema
-- MySQL 8.0+
-- Versão: 1.0
-- Data: 2025-10-26
-- ============================================

-- Criar database
CREATE DATABASE IF NOT EXISTS cobrio_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE cobrio_db;

-- ============================================
-- 1. Tabela: EmpresaCliente
-- ============================================
CREATE TABLE EmpresaCliente (
    Id CHAR(36) PRIMARY KEY,
    Nome VARCHAR(200) NOT NULL,
    CNPJ CHAR(14) NOT NULL UNIQUE,
    Email VARCHAR(100) NOT NULL,
    Telefone VARCHAR(20),
    PlanoCobrioId INT NOT NULL,
    DataContrato DATETIME NOT NULL,
    StatusContrato ENUM('Ativo', 'Suspenso', 'Cancelado') NOT NULL DEFAULT 'Ativo',
    Endereco_Logradouro VARCHAR(200),
    Endereco_Numero VARCHAR(20),
    Endereco_Complemento VARCHAR(100),
    Endereco_Bairro VARCHAR(100),
    Endereco_Cidade VARCHAR(100),
    Endereco_Estado CHAR(2),
    Endereco_CEP CHAR(8),
    Endereco_Pais VARCHAR(50) DEFAULT 'Brasil',
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_cnpj (CNPJ),
    INDEX idx_status (StatusContrato),
    INDEX idx_plano (PlanoCobrioId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 2. Tabela: UsuarioEmpresa
-- ============================================
CREATE TABLE UsuarioEmpresa (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Perfil ENUM('Admin', 'Operador', 'Visualizador') NOT NULL DEFAULT 'Operador',
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    UltimoAcesso DATETIME,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    UNIQUE KEY uk_email_empresa (Email, EmpresaClienteId),
    INDEX idx_tenant_email (EmpresaClienteId, Email),
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 3. Tabela: PlanoOferta
-- ============================================
CREATE TABLE PlanoOferta (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Descricao TEXT,
    TipoCiclo ENUM('Mensal', 'Trimestral', 'Semestral', 'Anual', 'Uso') NOT NULL DEFAULT 'Mensal',
    ValorCentavos BIGINT NOT NULL,
    Moeda CHAR(3) NOT NULL DEFAULT 'BRL',
    PeriodoTrial INT DEFAULT 0,
    LimiteUsuarios INT,
    PermiteDowngrade BOOLEAN NOT NULL DEFAULT TRUE,
    PermiteUpgrade BOOLEAN NOT NULL DEFAULT TRUE,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo),
    INDEX idx_tenant_nome (EmpresaClienteId, Nome)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 4. Tabela: Assinante
-- ============================================
CREATE TABLE Assinante (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    PlanoOfertaId CHAR(36) NOT NULL,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    CPFCNPJ VARCHAR(14),
    Telefone VARCHAR(20),
    Status ENUM('Ativo', 'AguardandoPagamento', 'Trial', 'Inadimplente', 'Suspenso', 'Cancelado') NOT NULL DEFAULT 'Ativo',
    DataInicio DATETIME NOT NULL,
    DataFimCiclo DATETIME NOT NULL,
    DataCancelamento DATETIME,
    MotivoCancelamento TEXT,
    EmTrial BOOLEAN NOT NULL DEFAULT FALSE,
    DataFimTrial DATETIME,
    DiaVencimento INT NOT NULL,
    ProximaCobranca DATETIME NOT NULL,
    Endereco_Logradouro VARCHAR(200),
    Endereco_Numero VARCHAR(20),
    Endereco_Complemento VARCHAR(100),
    Endereco_Bairro VARCHAR(100),
    Endereco_Cidade VARCHAR(100),
    Endereco_Estado CHAR(2),
    Endereco_CEP CHAR(8),
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (PlanoOfertaId) REFERENCES PlanoOferta(Id) ON DELETE RESTRICT,
    INDEX idx_tenant_status (EmpresaClienteId, Status),
    INDEX idx_tenant_email (EmpresaClienteId, Email),
    INDEX idx_tenant_proxima_cobranca (EmpresaClienteId, ProximaCobranca),
    INDEX idx_proxima_cobranca (ProximaCobranca),
    INDEX idx_plano (PlanoOfertaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 5. Tabela: MetodoPagamento
-- ============================================
CREATE TABLE MetodoPagamento (
    Id CHAR(36) PRIMARY KEY,
    AssinanteId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,
    Tipo ENUM('Cartao', 'Boleto', 'Pix', 'DebitoAutomatico') NOT NULL,
    TokenGateway VARCHAR(255),
    GatewayProvider VARCHAR(50),
    UltimosDigitos VARCHAR(4),
    Bandeira VARCHAR(30),
    NomeTitular VARCHAR(150),
    DataValidade DATE,
    Principal BOOLEAN NOT NULL DEFAULT FALSE,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_assinante (AssinanteId),
    INDEX idx_tenant_ativo (EmpresaClienteId, Ativo),
    INDEX idx_assinante_principal (AssinanteId, Principal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 6. Tabela: Fatura (com particionamento)
-- ============================================
CREATE TABLE Fatura (
    Id CHAR(36) NOT NULL,
    AssinanteId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,
    NumeroFatura VARCHAR(50) NOT NULL,
    ValorBrutoCentavos BIGINT NOT NULL,
    DescontoCentavos BIGINT NOT NULL DEFAULT 0,
    ImpostosCentavos BIGINT NOT NULL DEFAULT 0,
    ValorLiquidoCentavos BIGINT NOT NULL,
    Moeda CHAR(3) NOT NULL DEFAULT 'BRL',
    DataEmissao DATETIME NOT NULL,
    DataVencimento DATETIME NOT NULL,
    DataPagamento DATETIME,
    Status ENUM('Pendente', 'AguardandoPagamento', 'Pago', 'Falhou', 'Cancelado', 'Reembolsado') NOT NULL DEFAULT 'Pendente',
    MetodoPagamentoId CHAR(36),
    TransacaoIdGateway VARCHAR(255),
    Observacoes TEXT,
    LinkBoleto TEXT,
    QrCodePix TEXT,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (Id, DataEmissao),
    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (MetodoPagamentoId) REFERENCES MetodoPagamento(Id) ON DELETE SET NULL,
    INDEX idx_tenant_status (EmpresaClienteId, Status),
    INDEX idx_tenant_vencimento (EmpresaClienteId, DataVencimento),
    INDEX idx_assinante_status (AssinanteId, Status),
    INDEX idx_data_vencimento (DataVencimento),
    INDEX idx_status_vencimento (Status, DataVencimento)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
PARTITION BY RANGE (YEAR(DataEmissao)) (
    PARTITION p2024 VALUES LESS THAN (2025),
    PARTITION p2025 VALUES LESS THAN (2026),
    PARTITION p2026 VALUES LESS THAN (2027),
    PARTITION p_future VALUES LESS THAN MAXVALUE
);

-- ============================================
-- 7. Tabela: ItemFatura
-- ============================================
CREATE TABLE ItemFatura (
    Id CHAR(36) PRIMARY KEY,
    FaturaId CHAR(36) NOT NULL,
    Descricao VARCHAR(255) NOT NULL,
    Quantidade INT NOT NULL DEFAULT 1,
    ValorUnitarioCentavos BIGINT NOT NULL,
    ValorTotalCentavos BIGINT NOT NULL,
    TipoItem ENUM('Plano', 'AddOn', 'Ajuste', 'Desconto', 'Taxa') NOT NULL DEFAULT 'Plano',
    PlanoOfertaId CHAR(36),
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (PlanoOfertaId) REFERENCES PlanoOferta(Id) ON DELETE SET NULL,
    INDEX idx_fatura (FaturaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 8. Tabela: TentativaPagamento (com particionamento)
-- ============================================
CREATE TABLE TentativaPagamento (
    Id CHAR(36) NOT NULL,
    FaturaId CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,
    NumeroTentativa INT NOT NULL,
    DataTentativa DATETIME NOT NULL,
    Resultado ENUM('Sucesso', 'Falha', 'Processando', 'Cancelado') NOT NULL,
    CodigoErro VARCHAR(50),
    MensagemErro TEXT,
    TransacaoIdGateway VARCHAR(255),
    GatewayProvider VARCHAR(50),
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id, DataTentativa),
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

-- ============================================
-- 9. Tabela: ReguaDunningConfig
-- ============================================
CREATE TABLE ReguaDunningConfig (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL UNIQUE,
    NumeroMaximoTentativas INT NOT NULL DEFAULT 3,
    IntervalosDiasJson JSON NOT NULL,
    EnviarEmail BOOLEAN NOT NULL DEFAULT TRUE,
    EnviarSMS BOOLEAN NOT NULL DEFAULT FALSE,
    EnviarNotificacaoInApp BOOLEAN NOT NULL DEFAULT TRUE,
    DiasSuspensao INT NOT NULL DEFAULT 15,
    DiasCancelamento INT NOT NULL DEFAULT 30,
    HoraInicioRetry TIME DEFAULT '08:00:00',
    HoraFimRetry TIME DEFAULT '20:00:00',
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 10. Tabela: TemplateComunicacao
-- ============================================
CREATE TABLE TemplateComunicacao (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    Nome VARCHAR(100) NOT NULL,
    Tipo ENUM('PreVencimento', 'Vencimento', 'FalhaPagamento', 'AvisoSuspensao', 'AvisoCancelamento', 'BoasVindas', 'Confirmacao') NOT NULL,
    Canal ENUM('Email', 'SMS', 'InApp') NOT NULL,
    Assunto VARCHAR(200),
    CorpoTexto TEXT NOT NULL,
    CorpoHtml TEXT,
    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AtualizadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_tenant_tipo (EmpresaClienteId, Tipo, Canal)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 11. Tabela: LogComunicacao (com particionamento)
-- ============================================
CREATE TABLE LogComunicacao (
    Id CHAR(36) NOT NULL,
    EmpresaClienteId CHAR(36) NOT NULL,
    AssinanteId CHAR(36) NOT NULL,
    FaturaId CHAR(36),
    TipoContato ENUM('Email', 'SMS', 'InApp') NOT NULL,
    TemplateId CHAR(36),
    Destinatario VARCHAR(200) NOT NULL,
    Assunto VARCHAR(200),
    DataEnvio DATETIME NOT NULL,
    StatusEnvio ENUM('Enviado', 'Falhou', 'Bounce', 'Spam') NOT NULL DEFAULT 'Enviado',
    MensagemErro TEXT,
    DataAbertura DATETIME,
    DataClique DATETIME,
    NumeroAberturas INT DEFAULT 0,
    NumeroCliques INT DEFAULT 0,
    ProviderId VARCHAR(255),
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (Id, DataEnvio),
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    FOREIGN KEY (AssinanteId) REFERENCES Assinante(Id) ON DELETE CASCADE,
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

-- ============================================
-- 12. Tabela: WebhookLog
-- ============================================
CREATE TABLE WebhookLog (
    Id CHAR(36) PRIMARY KEY,
    EmpresaClienteId CHAR(36) NOT NULL,
    Evento VARCHAR(100) NOT NULL,
    URL VARCHAR(500) NOT NULL,
    PayloadJson JSON NOT NULL,
    StatusHTTP INT,
    RespostaBody TEXT,
    NumeroTentativas INT NOT NULL DEFAULT 1,
    DataEnvio DATETIME NOT NULL,
    DataProximaTentativa DATETIME,
    Sucesso BOOLEAN NOT NULL DEFAULT FALSE,
    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (EmpresaClienteId) REFERENCES EmpresaCliente(Id) ON DELETE CASCADE,
    INDEX idx_tenant_evento (EmpresaClienteId, Evento),
    INDEX idx_sucesso_proxima (Sucesso, DataProximaTentativa)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- 13. Tabela: AuditoriaLog (para triggers)
-- ============================================
CREATE TABLE AuditoriaLog (
    Id CHAR(36) PRIMARY KEY,
    EntidadeId CHAR(36) NOT NULL,
    TipoEntidade VARCHAR(50) NOT NULL,
    EmpresaClienteId CHAR(36),
    UsuarioId CHAR(36),
    Acao VARCHAR(100) NOT NULL,
    ValorAntigo TEXT,
    ValorNovo TEXT,
    DataAcao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IpOrigem VARCHAR(45),
    UserAgent TEXT,
    INDEX idx_entidade (EntidadeId, TipoEntidade),
    INDEX idx_tenant_data (EmpresaClienteId, DataAcao),
    INDEX idx_data (DataAcao)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- VIEWS
-- ============================================

-- View de Métricas Diárias
CREATE VIEW vw_MetricasDiarias AS
SELECT
    e.Id AS EmpresaClienteId,
    DATE(f.DataEmissao) AS Data,
    COUNT(DISTINCT a.Id) AS TotalAssinantesAtivos,
    COUNT(DISTINCT CASE WHEN f.Status = 'Pago' THEN f.Id END) AS FaturasPagas,
    COUNT(DISTINCT CASE WHEN f.Status = 'Falhou' THEN f.Id END) AS FaturasFalhadas,
    SUM(CASE WHEN f.Status = 'Pago' THEN f.ValorLiquidoCentavos ELSE 0 END) / 100.0 AS ReceitaDia,
    COUNT(DISTINCT CASE WHEN a.Status = 'Cancelado' AND DATE(a.DataCancelamento) = DATE(f.DataEmissao) THEN a.Id END) AS ChurnDia
FROM EmpresaCliente e
LEFT JOIN Assinante a ON e.Id = a.EmpresaClienteId
LEFT JOIN Fatura f ON e.Id = f.EmpresaClienteId
GROUP BY e.Id, DATE(f.DataEmissao);

-- ============================================
-- STORED PROCEDURES
-- ============================================

DELIMITER $$

-- Calcular MRR (Monthly Recurring Revenue)
CREATE PROCEDURE sp_CalcularMRR(IN p_EmpresaClienteId CHAR(36))
BEGIN
    SELECT
        COALESCE(SUM(po.ValorCentavos) / 100.0, 0) AS MRR
    FROM Assinante a
    INNER JOIN PlanoOferta po ON a.PlanoOfertaId = po.Id
    WHERE a.EmpresaClienteId = p_EmpresaClienteId
      AND a.Status = 'Ativo'
      AND po.TipoCiclo = 'Mensal';
END$$

-- Calcular Churn Rate do mês
CREATE PROCEDURE sp_CalcularChurnMensal(
    IN p_EmpresaClienteId CHAR(36),
    IN p_Mes INT,
    IN p_Ano INT
)
BEGIN
    DECLARE v_TotalInicio INT;
    DECLARE v_TotalCancelados INT;
    DECLARE v_ChurnRate DECIMAL(5,2);

    -- Total de assinantes ativos no início do mês
    SELECT COUNT(*)
    INTO v_TotalInicio
    FROM Assinante
    WHERE EmpresaClienteId = p_EmpresaClienteId
      AND Status IN ('Ativo', 'AguardandoPagamento', 'Inadimplente')
      AND DataInicio < CONCAT(p_Ano, '-', LPAD(p_Mes, 2, '0'), '-01');

    -- Total cancelados no mês
    SELECT COUNT(*)
    INTO v_TotalCancelados
    FROM Assinante
    WHERE EmpresaClienteId = p_EmpresaClienteId
      AND Status = 'Cancelado'
      AND YEAR(DataCancelamento) = p_Ano
      AND MONTH(DataCancelamento) = p_Mes;

    -- Calcular churn rate
    IF v_TotalInicio > 0 THEN
        SET v_ChurnRate = (v_TotalCancelados / v_TotalInicio) * 100;
    ELSE
        SET v_ChurnRate = 0;
    END IF;

    SELECT
        v_TotalInicio AS TotalAssinantesInicio,
        v_TotalCancelados AS TotalCancelados,
        v_ChurnRate AS ChurnRatePercentual;
END$$

DELIMITER ;

-- ============================================
-- DADOS INICIAIS (SEED)
-- ============================================

-- Criar empresa de exemplo
INSERT INTO EmpresaCliente (Id, Nome, CNPJ, Email, PlanoCobrioId, DataContrato, StatusContrato)
VALUES (UUID(), 'Empresa Demo', '12345678000190', 'contato@empresademo.com', 1, NOW(), 'Ativo');

-- Inserir configuração padrão de dunning para empresa demo
INSERT INTO ReguaDunningConfig (
    Id,
    EmpresaClienteId,
    NumeroMaximoTentativas,
    IntervalosDiasJson,
    EnviarEmail,
    EnviarSMS,
    DiasSuspensao,
    DiasCancelamento
)
SELECT
    UUID(),
    Id,
    3,
    JSON_ARRAY(1, 3, 7),
    TRUE,
    FALSE,
    15,
    30
FROM EmpresaCliente
WHERE CNPJ = '12345678000190';

-- ============================================
-- FIM DO SCRIPT
-- ============================================

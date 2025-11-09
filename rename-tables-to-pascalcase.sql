-- Script para padronizar nomes de tabelas em PascalCase
-- Execute este script no banco de dados cobriodb

USE cobriodb;

-- Renomear tabelas de lowercase para PascalCase
-- O script verifica se a tabela existe antes de renomear

-- EmpresaCliente
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'empresacliente');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE empresacliente TO EmpresaCliente',
    'SELECT ''Tabela EmpresaCliente já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- UsuarioEmpresa
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'usuarioempresa');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE usuarioempresa TO UsuarioEmpresa',
    'SELECT ''Tabela UsuarioEmpresa já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PlanoOferta
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'planooferta');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE planooferta TO PlanoOferta',
    'SELECT ''Tabela PlanoOferta já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Assinante
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'assinante');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE assinante TO Assinante',
    'SELECT ''Tabela Assinante já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- MetodoPagamento
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'metodopagamento');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE metodopagamento TO MetodoPagamento',
    'SELECT ''Tabela MetodoPagamento já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Fatura
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'fatura');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE fatura TO Fatura',
    'SELECT ''Tabela Fatura já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ItemFatura
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'itemfatura');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE itemfatura TO ItemFatura',
    'SELECT ''Tabela ItemFatura já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- TentativaPagamento
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'tentativapagamento');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE tentativapagamento TO TentativaPagamento',
    'SELECT ''Tabela TentativaPagamento já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ReguaDunningConfig
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'reguadunningconfig');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE reguadunningconfig TO ReguaDunningConfig',
    'SELECT ''Tabela ReguaDunningConfig já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RefreshToken
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'refreshtoken');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE refreshtoken TO RefreshToken',
    'SELECT ''Tabela RefreshToken já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- RegraCobranca
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'regracobranca');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE regracobranca TO RegraCobranca',
    'SELECT ''Tabela RegraCobranca já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Cobranca
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'cobranca');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE cobranca TO Cobranca',
    'SELECT ''Tabela Cobranca já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- HistoricoNotificacao
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'historiconotificacao');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE historiconotificacao TO HistoricoNotificacao',
    'SELECT ''Tabela HistoricoNotificacao já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- HistoricoImportacao
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'historicoimportacao');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE historicoimportacao TO HistoricoImportacao',
    'SELECT ''Tabela HistoricoImportacao já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Modulo
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'modulo');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE modulo TO Modulo',
    'SELECT ''Tabela Modulo já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Acao
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'acao');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE acao TO Acao',
    'SELECT ''Tabela Acao já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- PermissaoPerfil
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'permissaoperfil');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE permissaoperfil TO PermissaoPerfil',
    'SELECT ''Tabela PermissaoPerfil já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ConfiguracaoEmail
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'configuracaoemail');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE configuracaoemail TO ConfiguracaoEmail',
    'SELECT ''Tabela ConfiguracaoEmail já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- TemplateEmail
SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = DATABASE()
    AND BINARY table_name = 'templateemail');

SET @sql = IF(@table_exists > 0,
    'RENAME TABLE templateemail TO TemplateEmail',
    'SELECT ''Tabela TemplateEmail já está OK'' AS resultado');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SELECT 'Script executado com sucesso! Todas as tabelas foram padronizadas em PascalCase.' AS resultado;

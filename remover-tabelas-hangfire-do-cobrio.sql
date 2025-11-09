-- Script para remover tabelas do Hangfire do banco Cobrio
-- Execute no HeidiSQL no banco Cobrio

USE Cobrio;

-- Verificar quais tabelas do Hangfire existem
SHOW TABLES LIKE 'Hangfire%';

-- Se as tabelas aparecerem acima, execute os comandos abaixo para removê-las:

DROP TABLE IF EXISTS `HangfireState`;
DROP TABLE IF EXISTS `HangfireJobParameter`;
DROP TABLE IF EXISTS `HangfireJobQueue`;
DROP TABLE IF EXISTS `HangfireSet`;
DROP TABLE IF EXISTS `HangfireList`;
DROP TABLE IF EXISTS `HangfireHash`;
DROP TABLE IF EXISTS `HangfireCounter`;
DROP TABLE IF EXISTS `HangfireAggregatedCounter`;
DROP TABLE IF EXISTS `HangfireJob`;
DROP TABLE IF EXISTS `HangfireServer`;
DROP TABLE IF EXISTS `HangfireDistributedLock`;
DROP TABLE IF EXISTS `HangfireSchema`;

-- Verificar se foi tudo removido
SHOW TABLES LIKE 'Hangfire%';
SELECT 'Tabelas do Hangfire removidas do banco Cobrio com sucesso!' as Status;

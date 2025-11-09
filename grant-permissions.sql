-- Script para conceder permissões ao usuário root nos bancos Cobrio e cobrio_hangfire
-- Execute no MySQL Workbench ou HeidiSQL

-- Conceder todas as permissões no banco Cobrio
GRANT ALL PRIVILEGES ON Cobrio.* TO 'root'@'localhost';

-- Conceder todas as permissões no banco cobrio_hangfire
GRANT ALL PRIVILEGES ON cobrio_hangfire.* TO 'root'@'localhost';

-- Aplicar as mudanças
FLUSH PRIVILEGES;

-- Verificar permissões
SHOW GRANTS FOR 'root'@'localhost';

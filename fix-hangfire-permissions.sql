-- Conceder permissões ao usuário root no banco cobrio_hangfire
-- MySQL 8.0+ syntax
USE cobrio_hangfire;

-- Conceder permissões apenas para root@localhost (que já existe)
GRANT ALL PRIVILEGES ON cobrio_hangfire.* TO 'root'@'localhost';

-- Aplicar as mudanças
FLUSH PRIVILEGES;

-- Verificar se o banco existe e está acessível
SELECT 'Banco cobrio_hangfire configurado com sucesso!' as Status;
SHOW TABLES;

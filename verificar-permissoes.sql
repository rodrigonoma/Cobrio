-- Script para verificar permissões do usuário root
-- Execute no HeidiSQL

-- Ver todos os usuários root
SELECT user, host, plugin, authentication_string
FROM mysql.user
WHERE user = 'root';

-- Ver permissões do root@localhost
SHOW GRANTS FOR 'root'@'localhost';

-- Testar conexão com o banco Cobrio
USE Cobrio;
SELECT 'Conectado ao banco Cobrio com sucesso!' as Status;

-- Testar conexão com o banco cobrio_hangfire
USE cobrio_hangfire;
SELECT 'Conectado ao banco cobrio_hangfire com sucesso!' as Status;

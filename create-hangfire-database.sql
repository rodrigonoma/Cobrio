-- Criar banco de dados do Hangfire separado
CREATE DATABASE IF NOT EXISTS cobrio_hangfire
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

-- Verificar se foi criado
SHOW DATABASES LIKE 'cobrio%';

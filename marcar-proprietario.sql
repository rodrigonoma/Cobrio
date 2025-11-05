-- Script para marcar o usuário admin como proprietário
-- Execute este script no MySQL Workbench ou outro cliente MySQL

USE Cobrio;

-- Atualizar o usuário admin existente para ser proprietário
UPDATE UsuarioEmpresa
SET EhProprietario = 1
WHERE Email = 'admin@empresademo.com.br';

-- Verificar se a atualização funcionou
SELECT
    Id,
    Nome,
    Email,
    Perfil,
    EhProprietario,
    Ativo
FROM UsuarioEmpresa
WHERE Email = 'admin@empresademo.com.br';

# 📋 INSTRUÇÕES: Adicionar Módulo "Relatório de Consumo"

## Problema

O módulo "Relatório de Consumo" foi criado no código, mas **não aparece na tela de Gerenciar Permissões** porque não foi adicionado na tabela `Modulo` do banco de dados.

## Solução

Execute o script SQL no servidor VPS para adicionar o módulo.

## Passo a Passo

### 1. Fazer Upload do Arquivo SQL

Copie o arquivo `adicionar-modulo-relatorio-consumo.sql` para o VPS:

```bash
# Opção 1: Via SCP (do Windows)
scp C:\Cobrio\adicionar-modulo-relatorio-consumo.sql seu-usuario@72.60.63.64:/tmp/

# Opção 2: Via WinSCP, FileZilla ou outro cliente FTP
# Upload para: /tmp/adicionar-modulo-relatorio-consumo.sql
```

### 2. Conectar no VPS via SSH

```bash
ssh seu-usuario@72.60.63.64
```

### 3. Executar o Script SQL

```bash
# No VPS, executar:
mysql -h 72.60.63.64 -u cobrio_user -p cobrio < /tmp/adicionar-modulo-relatorio-consumo.sql

# Quando pedir a senha, digite: A$HAi8hA82%
```

### 4. Verificar se Funcionou

Após executar, você deve ver mensagens como:

```
Módulo inserido com sucesso!
Permissões criadas com sucesso!
Pronto! Agora o módulo "Relatório de Consumo" deve aparecer na tela de Gerenciar Permissões.
```

## Alternativa: Executar SQL Manualmente

Se preferir, conecte no MySQL e execute linha por linha:

```bash
# No VPS
mysql -h 72.60.63.64 -u cobrio_user -p cobrio
```

Depois, copie e cole este SQL:

```sql
-- Inserir módulo
INSERT INTO Modulo (
    Id, Nome, Chave, NomeAmigavel, Descricao, Ordem, Ativo, CriadoEm, ModificadoEm
)
SELECT
    UUID(),
    'Relatório de Consumo',
    'relatorio-consumo',
    'Consumo de Canais',
    'Acompanhamento do consumo de canais de notificação (Email, SMS, WhatsApp)',
    COALESCE(MAX(Ordem), 0) + 1,
    1,
    NOW(),
    NOW()
FROM Modulo
WHERE NOT EXISTS (SELECT 1 FROM Modulo WHERE Chave = 'relatorio-consumo');

-- Criar permissão para Admin
SET @moduloId = (SELECT Id FROM Modulo WHERE Chave = 'relatorio-consumo');

INSERT INTO Permissao (Id, Perfil, ModuloId, Acao, Permitido, CriadoEm, ModificadoEm)
SELECT UUID(), 'Admin', @moduloId, 'read', 1, NOW(), NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM Permissao WHERE Perfil = 'Admin' AND ModuloId = @moduloId AND Acao = 'read'
);

-- Verificar
SELECT m.Nome, m.Chave, m.Ordem, m.Ativo
FROM Modulo m
WHERE m.Chave = 'relatorio-consumo';
```

## Testando

Após executar o SQL:

1. **Acesse o sistema** e faça login
2. Vá em **Configurações → Gerenciar Permissões**
3. Você deve ver agora **3 módulos de relatórios**:
   - ✅ Relatórios Operacionais
   - ✅ Relatórios Gerenciais
   - ✅ **Relatório de Consumo** (NOVO!)

4. Configure as permissões conforme necessário para cada perfil

## Estrutura do Módulo Criado

```
Nome: Relatório de Consumo
Chave: relatorio-consumo
Nome Amigável: Consumo de Canais
Descrição: Acompanhamento do consumo de canais de notificação
Ativo: Sim
Ordem: (próxima ordem disponível)
```

## Troubleshooting

### O módulo não aparece na tela?

1. Limpe o cache do navegador (Ctrl + F5)
2. Faça logout e login novamente
3. Verifique se o SQL foi executado com sucesso:
   ```sql
   SELECT * FROM Modulo WHERE Chave = 'relatorio-consumo';
   ```

### Erro "Duplicate entry"?

Significa que o módulo já existe. Execute apenas:
```sql
SELECT Id, Nome, Chave FROM Modulo WHERE Chave = 'relatorio-consumo';
```
Se retornar um registro, está tudo OK!

### Permissão não funciona?

Verifique se a permissão foi criada:
```sql
SELECT p.*, m.Nome
FROM Permissao p
INNER JOIN Modulo m ON p.ModuloId = m.Id
WHERE m.Chave = 'relatorio-consumo';
```

---

**Criado em**: 12/11/2025 09:30
**Status**: ⏳ Aguardando Execução no VPS

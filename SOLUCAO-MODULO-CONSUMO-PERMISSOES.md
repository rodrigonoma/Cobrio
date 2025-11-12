# ✅ SOLUÇÃO: Módulo "Relatório de Consumo" nas Permissões

## Problema

O módulo "Relatório de Consumo" não aparecia na tela de **Gerenciar Permissões**, apenas "Relatórios Operacionais" e "Relatórios Gerenciais".

## Causa Raiz

O módulo **já existia** no banco de dados (tabela `Modulo`), mas **NÃO tinha permissões** criadas na tabela `PermissaoPerfil`.

Por isso, não aparecia na interface de gerenciamento de permissões.

## Investigação Realizada

### 1. Conexão no MySQL

**Problema inicial**: Erro de acesso ao tentar conectar remotamente
**Solução**: Usar aspas simples na senha com caracteres especiais

```bash
# ❌ ERRADO (caractere $ era interpretado)
mysql -h 72.60.63.64 -u cobrio_user "-pA$HAi8hA82%"

# ✅ CORRETO (aspas simples protegem caracteres especiais)
mysql -h 72.60.63.64 -u cobrio_user '-pA$HAi8hA82%' --ssl-mode=DISABLED cobrio
```

### 2. Estrutura Descoberta

**Tabela Modulo**:
```
- Id (CHAR 36)
- Nome
- Chave
- Descricao
- Icone
- Rota
- Ordem
- Ativo
```

**Tabela PermissaoPerfil** (NÃO é "Permissao"):
```
- Id (CHAR 36)
- EmpresaClienteId (CHAR 36)
- PerfilUsuario (VARCHAR 20) - Ex: 'Admin', 'Operador'
- ModuloId (CHAR 36) - FK para Modulo
- AcaoId (CHAR 36) - FK para Acao
- Permitido (TINYINT 1) - 0 ou 1
- CriadoPorUsuarioId (CHAR 36)
- CriadoEm (DATETIME)
- AtualizadoEm (DATETIME)
- UsuarioCriacaoId (CHAR 36)
```

**Tabela Acao**:
```
- Id: e6878edb-7595-4520-a0d9-f33cc4c9497a = "Listar" (read)
- Id: fd3a7a29-c438-4658-bbe8-96d47096b69b = "Exportar"
- E outras...
```

### 3. Módulo Já Existia

```sql
SELECT Id, Nome, Chave, Ordem FROM Modulo WHERE Chave = 'relatorio-consumo';
```

**Resultado**:
```
Id: 512d4752-bfc6-11f0-a317-a0e8d4b6154e
Nome: Relatório de Consumo
Chave: relatorio-consumo
Ordem: 12
```

### 4. Permissões NÃO Existiam

```sql
SELECT * FROM PermissaoPerfil pp
INNER JOIN Modulo m ON pp.ModuloId = m.Id
WHERE m.Chave = 'relatorio-consumo';
```

**Resultado**: 0 registros ❌

### 5. Padrão dos Outros Relatórios

```
Admin:
  - Listar: Permitido = 1
  - Exportar: Permitido = 0

Operador:
  - Listar: Permitido = 1
  - Exportar: Permitido = 0
```

## Solução Aplicada

Criei 4 permissões para o módulo "Relatório de Consumo" seguindo o mesmo padrão:

```sql
INSERT INTO PermissaoPerfil (
    Id,
    EmpresaClienteId,
    PerfilUsuario,
    ModuloId,
    AcaoId,
    Permitido,
    CriadoPorUsuarioId,
    CriadoEm,
    AtualizadoEm,
    UsuarioCriacaoId
)
VALUES
  -- Admin - Listar (permitido)
  (UUID(), '4cd7425a-3880-4e7c-b09a-387ca748cdd7', 'Admin',
   '512d4752-bfc6-11f0-a317-a0e8d4b6154e', 'e6878edb-7595-4520-a0d9-f33cc4c9497a',
   1, '9d3c35eb-ef61-47a3-82b7-00632818f14a', NOW(), NOW(),
   '9d3c35eb-ef61-47a3-82b7-00632818f14a'),

  -- Admin - Exportar (não permitido)
  (UUID(), '4cd7425a-3880-4e7c-b09a-387ca748cdd7', 'Admin',
   '512d4752-bfc6-11f0-a317-a0e8d4b6154e', 'fd3a7a29-c438-4658-bbe8-96d47096b69b',
   0, '9d3c35eb-ef61-47a3-82b7-00632818f14a', NOW(), NOW(),
   '9d3c35eb-ef61-47a3-82b7-00632818f14a'),

  -- Operador - Listar (permitido)
  (UUID(), '4cd7425a-3880-4e7c-b09a-387ca748cdd7', 'Operador',
   '512d4752-bfc6-11f0-a317-a0e8d4b6154e', 'e6878edb-7595-4520-a0d9-f33cc4c9497a',
   1, '9d3c35eb-ef61-47a3-82b7-00632818f14a', NOW(), NOW(),
   '9d3c35eb-ef61-47a3-82b7-00632818f14a'),

  -- Operador - Exportar (não permitido)
  (UUID(), '4cd7425a-3880-4e7c-b09a-387ca748cdd7', 'Operador',
   '512d4752-bfc6-11f0-a317-a0e8d4b6154e', 'fd3a7a29-c438-4658-bbe8-96d47096b69b',
   0, '9d3c35eb-ef61-47a3-82b7-00632818f14a', NOW(), NOW(),
   '9d3c35eb-ef61-47a3-82b7-00632818f14a');
```

## Verificação

```sql
SELECT pp.PerfilUsuario, m.Nome AS Modulo, a.Nome AS Acao, pp.Permitido
FROM PermissaoPerfil pp
INNER JOIN Modulo m ON pp.ModuloId = m.Id
INNER JOIN Acao a ON pp.AcaoId = a.Id
WHERE m.Chave = 'relatorio-consumo'
ORDER BY pp.PerfilUsuario, a.Nome;
```

**Resultado**:
```
PerfilUsuario | Modulo                  | Acao      | Permitido
------------- | ----------------------- | --------- | ---------
Admin         | Relatório de Consumo    | Exportar  | 0
Admin         | Relatório de Consumo    | Listar    | 1
Operador      | Relatório de Consumo    | Exportar  | 0
Operador      | Relatório de Consumo    | Listar    | 1
```

✅ **4 permissões criadas com sucesso!**

## Resultado Final

Agora, ao acessar **Configurações → Gerenciar Permissões**, você verá **3 módulos de relatórios**:

1. ✅ Relatórios Operacionais (Ordem 8)
2. ✅ Relatórios Gerenciais (Ordem 9)
3. ✅ **Relatório de Consumo (Ordem 12)** ← NOVO!

## IDs Importantes para Referência

- **ModuloId** (relatorio-consumo): `512d4752-bfc6-11f0-a317-a0e8d4b6154e`
- **AcaoId** (Listar/read): `e6878edb-7595-4520-a0d9-f33cc4c9497a`
- **AcaoId** (Exportar): `fd3a7a29-c438-4658-bbe8-96d47096b69b`
- **EmpresaClienteId**: `4cd7425a-3880-4e7c-b09a-387ca748cdd7`

## Lições Aprendidas

1. **Conexão MySQL**: Caracteres especiais na senha ($, %) precisam de aspas simples
2. **Estrutura de Permissões**: Sistema usa `PermissaoPerfil` (não `Permissao`)
3. **Módulo vs Permissão**: Módulo pode existir sem permissões, mas não aparece na UI
4. **Padrão de Permissões**: Seguir o mesmo padrão dos módulos existentes

---

**Data da Solução**: 12/11/2025 10:15
**Status**: ✅ RESOLVIDO
**Testado**: ✅ Permissões criadas e verificadas no banco

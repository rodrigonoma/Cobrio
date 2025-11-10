# Como Sincronizar Módulos e Permissões

## Problema Resolvido

Antes, ao adicionar um novo módulo ao sistema (como Templates), era necessário:
1. Criar scripts SQL manualmente
2. Executar em cada ambiente (local, produção)
3. Garantir que todas as empresas recebessem as permissões

Isso era trabalhoso e propenso a erros.

## Solução Automática

Agora existe um endpoint de API que **sincroniza automaticamente** módulos e permissões!

### O que ele faz?

O endpoint `POST /api/Admin/sync-permissions`:
- ✅ Adiciona módulos que ainda não existem
- ✅ Adiciona ações que ainda não existem
- ✅ Adiciona permissões faltantes para **todas as empresas**
- ✅ É **idempotente** (pode executar múltiplas vezes sem problemas)
- ✅ Não duplica dados existentes

### Como Usar

#### 1. Obter um token de autenticação

Faça login no sistema e obtenha o token JWT.

#### 2. Chamar o endpoint de sincronização

**LOCAL (desenvolvimento):**
```bash
curl -X POST "http://localhost:5271/api/Admin/sync-permissions" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -v
```

**PRODUÇÃO:**
```bash
curl -X POST "https://seu-dominio.com/api/Admin/sync-permissions" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI" \
  -v
```

#### 3. Verificar o status (opcional)

```bash
curl -X GET "http://localhost:5271/api/Admin/permissions-status" \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

Resposta:
```json
{
  "totalModulos": 11,
  "totalAcoes": 12,
  "totalEmpresas": 1,
  "totalPermissoes": 75,
  "modulosExistentes": [
    { "chave": "dashboard", "nome": "Dashboard" },
    { "chave": "assinaturas", "nome": "Assinaturas" },
    { "chave": "templates", "nome": "Templates" },
    ...
  ]
}
```

## Quando Usar?

Execute o endpoint de sincronização sempre que:

1. **Adicionar um novo módulo** ao sistema
2. **Adicionar novas ações** ao sistema
3. **Criar uma nova empresa** e quiser garantir que ela tenha todas as permissões padrão
4. **Após deploy em produção** de mudanças relacionadas a módulos/permissões
5. **Se algo estiver faltando** e você não souber exatamente o quê

## Como Adicionar um Novo Módulo?

### 1. Atualizar o PermissaoSeeder.cs

Edite `src/Cobrio.Infrastructure/Data/PermissaoSeeder.cs`:

```csharp
// No método SyncModulosEPermissoesAsync(), adicionar na lista de módulos:
("Novo Módulo", "novo-modulo", "Descrição do módulo", "pi-icon", "/rota", 12),

// Adicionar no moduloAcoesMap:
["novo-modulo"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete" },
```

### 2. Atualizar o PermissoesController.cs

**IMPORTANTE:** Também precisa adicionar no controller de permissões!

Edite `src/Cobrio.API/Controllers/PermissoesController.cs` (linha ~105):

```csharp
var moduloAcoesMap = new Dictionary<string, string[]>
{
    // ... módulos existentes ...
    ["novo-modulo"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete" },
};
```

**Se não fizer isso, o módulo aparecerá na tela de Permissões mas SEM checkboxes!**

### 2. Compilar e fazer deploy

```bash
# Build da API
dotnet build src/Cobrio.API

# Deploy (seguir processo normal)
```

### 3. Executar sincronização

```bash
# Em cada ambiente (local, produção), chamar:
curl -X POST "http://localhost:5271/api/Admin/sync-permissions" \
  -H "Authorization: Bearer SEU_TOKEN"
```

**Pronto!** Todas as empresas terão as permissões do novo módulo automaticamente.

## Regras de Permissão Padrão

### Admin (PerfilUsuario = 1)
- Acesso **total** a todos os módulos
- **Exceto**: Permissões e Configurações (apenas Proprietário)

### Operador (PerfilUsuario = 2)
- **Templates**: Visualizar apenas (menu.view, read, read.details)
- **Regras de Cobrança**: Visualizar apenas (menu.view, read, read.details)
- **Outros módulos**: Sem acesso

### Proprietário (PerfilUsuario = 0)
- Acesso **total** a tudo, incluindo Permissões e Configurações

## Diferença Entre os Métodos

### `SeedModulosEAcoesAsync()` e `SeedPermissoesDefaultAsync()`
- **Quando**: Executado automaticamente no primeiro uso do banco
- **Problema**: Só funciona se o banco estiver vazio
- **Não usa mais** para adicionar coisas novas

### `SyncModulosEPermissoesAsync()` ⭐ NOVO
- **Quando**: Executado manualmente via API
- **Vantagem**: Pode executar a qualquer momento
- **Idempotente**: Não duplica dados
- **Recomendado** para todas as situações

## Exemplos Práticos

### Exemplo 1: Adicionar módulo de Notificações

1. Editar `PermissaoSeeder.cs`:
```csharp
("Notificações", "notificacoes", "Gerenciar notificações", "pi-bell", "/notificacoes", 12),
```

2. Build e deploy

3. Sincronizar:
```bash
curl -X POST "http://localhost:5271/api/Admin/sync-permissions" -H "Authorization: Bearer $TOKEN"
```

### Exemplo 2: Verificar se tudo está OK

```bash
curl -X GET "http://localhost:5271/api/Admin/permissions-status" -H "Authorization: Bearer $TOKEN"
```

Se `totalModulos` for 11 (ou o esperado), está tudo certo!

## Notas Importantes

1. **Não precisa mais criar scripts SQL manuais** para permissões
2. **Não precisa mais se preocupar** com empresas que não têm permissões
3. **Pode executar quantas vezes quiser** - é seguro
4. **Funciona em qualquer ambiente** - local, produção, staging, etc.
5. **Lembre-se de autenticar** - o endpoint requer um token válido

## Troubleshooting

### "401 Unauthorized"
- Certifique-se de que o token está válido
- Faça login novamente se necessário

### "500 Internal Server Error"
- Verifique os logs da API
- Pode ser um problema com o banco de dados

### Permissões não aparecem no sistema
- Execute o endpoint de sincronização
- Verifique se o módulo está definido corretamente no PermissaoSeeder.cs
- Limpe o cache do navegador e faça logout/login

## Resumo

✅ **Antes**: Scripts SQL manuais, trabalhoso, propenso a erros

✅ **Agora**: Um único comando via API, automático, seguro, idempotente

```bash
curl -X POST "http://localhost:5271/api/Admin/sync-permissions" \
  -H "Authorization: Bearer $TOKEN"
```

**Fim da dificuldade!** 🎉

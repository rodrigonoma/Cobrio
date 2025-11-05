# 🔐 Sistema de Permissões - Guia Completo

## ✅ Status: 100% Implementado

### 📋 Estrutura

#### Backend (.NET)
- ✅ Entidades: `Modulo`, `Acao`, `PermissaoPerfil`
- ✅ Repositories com cache (30 minutos)
- ✅ Services com validações
- ✅ Controllers com endpoints REST
- ✅ Seeds automáticos

#### Frontend (Angular)
- ✅ Service de permissões
- ✅ Componente de gerenciamento
- ✅ Sidebar dinâmica
- ✅ Componentes com controle de ações

---

## 🚀 Como Usar

### 1. Login

**Proprietário (Admin com EhProprietario = true):**
- Email: `admin@empresademo.com.br`
- Senha: `Admin@123`
- Perfil: `Admin`
- Pode acessar TUDO, incluindo tela de Permissões

### 2. Acessar Tela de Permissões

Após login como proprietário:
1. Vá em **Permissões** no menu lateral
2. Selecione um perfil (Admin ou Operador)
3. Marque/desmarque as permissões desejadas
4. Clique em **Salvar**

### 3. Estrutura de Permissões

**8 Módulos:**
1. Dashboard
2. Assinaturas
3. Planos
4. Financeiro
5. Regras de Cobrança
6. Usuários
7. Relatórios
8. Permissões

**11 Ações:**
- **Menu:** `menu.view` (Visualizar no menu lateral)
- **CRUD:** `read`, `create`, `update`, `delete`, `read.details`, `toggle`
- **Especiais:** `export`, `import`, `reset-password`, `config-permissions`

---

## 🔧 Configuração Padrão (Seeds)

### Perfil Admin
- ✅ Acesso a **todos** os módulos (exceto Permissões)
- ✅ Todas as ações CRUD e especiais
- ❌ **NÃO** pode acessar módulo "Permissões"

### Perfil Operador
- ✅ Apenas módulo "Regras de Cobrança"
- ✅ Apenas ação `menu.view` e `read` (visualização)
- ❌ Não pode criar, editar ou excluir

### Proprietário (flag especial)
- ✅ Acesso a **TUDO** (incluindo Permissões)
- ✅ Únicoque pode configurar permissões
- ✅ Badge especial na lista de usuários
- ❌ Não pode ser editado ou excluído por outros

---

## 🎯 Endpoints da API

### GET /api/permissoes/modulos
Retorna todos os módulos ativos.

### GET /api/permissoes/acoes
Retorna todas as ações ativas.

### GET /api/permissoes/perfil/{perfil}
Retorna permissões configuradas para um perfil.
- Parâmetro: `Admin` ou `Operador`

### GET /api/permissoes/verificar
Verifica se um perfil tem uma permissão específica.
- Query params: `perfil`, `moduloChave`, `acaoChave`

### POST /api/permissoes/configurar
Configura permissões de um perfil (apenas Proprietário).
```json
{
  "perfilUsuario": "Admin",
  "permissoes": {
    "moduloId-guid": {
      "acaoId-guid": true,
      "acaoId-guid": false
    }
  }
}
```

---

## 💡 Exemplos de Uso

### Cenário 1: Permitir que Operador crie regras
1. Login como Proprietário
2. Acesse "Permissões"
3. Selecione perfil "Operador"
4. No módulo "Regras de Cobrança", marque a ação "Criar"
5. Salve
6. ✅ Operador poderá criar regras!

### Cenário 2: Dar acesso a Dashboard para Operador
1. Login como Proprietário
2. Acesse "Permissões"
3. Selecione perfil "Operador"
4. No módulo "Dashboard", marque "Visualizar Menu"
5. Marque também as ações que ele pode fazer no Dashboard
6. Salve
7. ✅ Operador verá Dashboard no menu!

### Cenário 3: Remover acesso de Admin a Financeiro
1. Login como Proprietário
2. Acesse "Permissões"
3. Selecione perfil "Admin"
4. No módulo "Financeiro", desmarque "Visualizar Menu"
5. Salve
6. ✅ Admins não verão mais Financeiro no menu!

---

## 🐛 Troubleshooting

### Menu não aparece após login
1. Abra console do navegador (F12)
2. Verifique se há erros de requisição
3. Confirme que as permissões foram seedadas:
   ```bash
   # No backend, logs devem mostrar:
   [INF] Carregando módulos ativos do banco de dados
   [INF] Carregando permissões do perfil Admin
   ```

### Permissões não estão sendo salvas
1. Verifique se você está logado como Proprietário
2. O endpoint `/api/permissoes/configurar` só funciona para Proprietário
3. Verifique se `EhProprietario = true` no banco:
   ```sql
   SELECT * FROM UsuarioEmpresa WHERE EhProprietario = 1;
   ```

### Como resetar permissões
1. Delete as permissões do banco:
   ```sql
   DELETE FROM PermissaoPerfil WHERE EmpresaClienteId = 'seu-id';
   ```
2. Reinicie o backend
3. O seed recriará as permissões padrão

---

## 📊 Arquitetura

### Cache (Performance)
- **Backend:** Memory Cache de 30 minutos
- **Chaves:** `permissoes_{empresaId}_{perfil}`
- **Invalidação:** Automática ao salvar novas permissões

### Multi-Tenant
- Todas as permissões são isoladas por `EmpresaClienteId`
- Queries automáticas com filtro de tenant

### Segurança
- Proprietário: Acesso total via flag `EhProprietario`
- Admin: Controlado por permissões no banco
- Operador: Controlado por permissões no banco

---

## 🔄 Fluxo de Verificação

```
1. Usuário faz login
   ↓
2. Frontend recebe perfil (Admin/Operador)
   ↓
3. Sidebar carrega e faz chamada para cada módulo:
   GET /api/permissoes/verificar?perfil=Admin&moduloChave=dashboard&acaoChave=menu.view
   ↓
4. Backend verifica no banco (com cache)
   ↓
5. Retorna { "permitido": true/false }
   ↓
6. Frontend mostra/oculta item do menu
```

---

## ✨ Recursos Avançados

### Criar Novo Módulo
1. Adicione no seed (`PermissaoSeeder.cs`)
2. Execute migration
3. Adicione no sidebar do Angular
4. Configure permissões pela tela!

### Criar Nova Ação
1. Adicione no seed como `TipoAcao.Especial`
2. Execute migration
3. Use em guards ou componentes
4. Configure permissões pela tela!

---

## 📝 Logs Importantes

Backend (Serilog):
```
[INF] Carregando módulos ativos do banco de dados
[INF] Carregando permissões do perfil Admin da empresa {EmpresaId}
[INF] Permissões configuradas com sucesso para perfil Admin. Total: 56
```

Frontend (Console):
```
[Sidebar] Carregando permissões para perfil: Admin
[Sidebar] Dashboard: true
[Sidebar] Usuários: true
[Sidebar] Permissões: false  (Admin não tem acesso)
```

---

🎉 **Sistema 100% Funcional e Data-Driven!**

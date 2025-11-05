# 🔐 Proposta: Sistema de Gerenciamento de Múltiplos Usuários por Empresa

## 📋 Problema Atual

**Situação:** Uma empresa contrata o serviço e tem apenas um login. Se a equipe precisa usar o sistema, todos compartilham o mesmo usuário/senha.

**Problemas:**
- ❌ Compartilhamento de credenciais (má prática de segurança)
- ❌ Impossível saber quem fez cada ação (auditoria)
- ❌ Não há controle de permissões por pessoa
- ❌ Se um funcionário sai da empresa, precisa trocar a senha de todos

---

## ✅ Solução Proposta

### 1. **Como evitar compartilhamento de login?**

**Resposta:** Implementar sistema onde o usuário master pode convidar membros da equipe, cada um com seu próprio login.

#### Medidas de segurança:
- 🔑 **Cada pessoa tem suas próprias credenciais** (email/senha único)
- 👤 **Auditoria completa** - registrar quem fez cada ação
- 🚫 **Controle de acesso** - permissões baseadas em perfil/role
- 🔒 **Políticas de senha forte** - mínimo 8 caracteres, complexidade
- 📊 **Relatório de atividades** - histórico por usuário
- ⏱️ **Sessão única** (opcional) - limitar logins simultâneos
- 🔐 **2FA** (futuro) - autenticação de dois fatores

---

## 🏗️ Arquitetura Proposta

### **Hierarquia de Usuários**

```
EmpresaCliente (Tenant)
  └── UsuarioEmpresa (Admin) ← Usuário Master (quem contratou)
        ├── UsuarioEmpresa (Operador) ← Membro da equipe
        ├── UsuarioEmpresa (Operador) ← Membro da equipe
        └── UsuarioEmpresa (Visualizador) ← Membro da equipe
```

### **Perfis e Permissões**

| Perfil | Permissões |
|--------|------------|
| **Admin** | ✅ Gerenciar usuários<br>✅ Configurar regras de cobrança<br>✅ Ver relatórios completos<br>✅ Importar cobranças<br>✅ Configurações da empresa |
| **Operador** | ✅ Criar/editar regras de cobrança<br>✅ Importar cobranças<br>✅ Ver relatórios<br>❌ Gerenciar usuários<br>❌ Alterar configurações da empresa |
| **Visualizador** | ✅ Ver regras de cobrança<br>✅ Ver relatórios<br>❌ Criar/editar regras<br>❌ Importar cobranças<br>❌ Gerenciar usuários |

---

## 🎯 Funcionalidades a Implementar

### **1. Gerenciamento de Usuários (CRUD)**

#### Backend:
- ✅ **Já existe:** Entidade `UsuarioEmpresa` e enum `PerfilUsuario`
- 🆕 **Criar:** Controller `UsuarioEmpresaController`
- 🆕 **Criar:** Service `UsuarioEmpresaService`
- 🆕 **Criar:** Repository `IUsuarioEmpresaRepository`

#### Frontend:
- 🆕 **Tela:** Listagem de usuários da empresa
- 🆕 **Tela:** Formulário para convidar/adicionar usuário
- 🆕 **Tela:** Editar usuário (nome, perfil)
- 🆕 **Ação:** Desativar/reativar usuário
- 🆕 **Ação:** Resetar senha do usuário

---

### **2. Sistema de Convites (Recomendado)**

**Fluxo:**
1. Admin clica em "Convidar usuário"
2. Digita email + seleciona perfil (Operador/Visualizador)
3. Sistema envia email com link de convite
4. Novo usuário clica no link, define seu nome e senha
5. Usuário já pode fazer login

**Benefícios:**
- ✅ Cada pessoa define sua própria senha (mais seguro)
- ✅ Email verificado automaticamente
- ✅ Experiência profissional

**Alternativa Simples (mais rápida de implementar):**
- Admin cria usuário diretamente com senha temporária
- Sistema força troca de senha no primeiro login

---

### **3. Auditoria e Histórico**

**Campos a adicionar nas entidades principais:**

```csharp
// Em RegraCobranca, Cobranca, etc.
public Guid CriadoPorUsuarioId { get; private set; }
public Guid? AtualizadoPorUsuarioId { get; private set; }
```

**Telas:**
- 🆕 **Relatório de ações por usuário**
- 🆕 **Filtro de histórico por usuário**
- 🆕 **Log de atividades** (quem criou/editou cada regra)

---

### **4. Restrições de Perfil**

**No Backend (Authorization):**
```csharp
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateUsuario(...)

[Authorize(Roles = "Admin,Operador")]
public async Task<IActionResult> ImportarCobrancas(...)
```

**No Frontend (UI):**
```typescript
// Esconder botões baseado no perfil do usuário logado
*ngIf="usuarioLogado.perfil === PerfilUsuario.Admin"
```

---

## 📐 Estrutura de Dados Necessária

### **1. Tabela: UsuarioEmpresa** ✅ (já existe)

```sql
UsuarioEmpresa
  - Id (PK)
  - EmpresaClienteId (FK)
  - Nome
  - Email
  - PasswordHash
  - Perfil (Admin=1, Operador=2, Visualizador=3)
  - Ativo
  - UltimoAcesso
  - CriadoEm
  - AtualizadoEm
```

### **2. Tabela: ConviteUsuario** 🆕 (opcional, se usar sistema de convites)

```sql
ConviteUsuario
  - Id (PK)
  - EmpresaClienteId (FK)
  - Email
  - Perfil
  - Token (GUID único)
  - ConvidadoPorUsuarioId (FK)
  - DataConvite
  - DataExpiracao
  - Aceito (bool)
  - DataAceite
```

### **3. Auditoria em entidades existentes** 🆕

Adicionar campos de auditoria em:
- `RegraCobranca`
- `Cobranca`
- `HistoricoImportacao`

```sql
-- Adicionar em cada tabela
CriadoPorUsuarioId (FK → UsuarioEmpresa.Id)
AtualizadoPorUsuarioId (FK → UsuarioEmpresa.Id)
```

---

## 🚀 Plano de Implementação

### **Fase 1: MVP (Mínimo Viável) - Gerenciamento Básico**

**Backend:**
1. ✅ Repository `IUsuarioEmpresaRepository`
2. ✅ Service `UsuarioEmpresaService`
3. ✅ Controller `UsuarioEmpresaController` com endpoints:
   - `GET /api/usuario-empresa` - Listar usuários da empresa
   - `POST /api/usuario-empresa` - Criar novo usuário
   - `PUT /api/usuario-empresa/{id}` - Editar usuário
   - `DELETE /api/usuario-empresa/{id}` - Desativar usuário
   - `POST /api/usuario-empresa/{id}/resetar-senha` - Resetar senha

**Frontend:**
1. ✅ Model `usuario-empresa.models.ts`
2. ✅ Service `usuario-empresa.service.ts`
3. ✅ Componente `usuarios-list.component` (listagem)
4. ✅ Componente `usuario-form.component` (criar/editar)
5. ✅ Rota protegida (apenas Admin)

**Segurança:**
1. ✅ Middleware de autorização por perfil
2. ✅ Validação de permissões no backend
3. ✅ Guards no frontend

---

### **Fase 2: Sistema de Convites (Opcional)**

**Backend:**
1. Criar entidade `ConviteUsuario`
2. Service `ConviteService`
3. Endpoints de convite
4. Integração com Brevo para enviar emails

**Frontend:**
1. Modal de "Convidar usuário"
2. Tela de aceite de convite (pública)

---

### **Fase 3: Auditoria Completa**

**Backend:**
1. Adicionar campos de auditoria nas entidades
2. Migration para adicionar colunas
3. Atualizar services para registrar criador/modificador

**Frontend:**
1. Filtros por usuário nos relatórios
2. Coluna "Criado por" nas listagens
3. Histórico de atividades

---

## 🔄 Fluxo do Usuário Master

### **Cenário: Adicionar novo membro da equipe**

1. **Master faz login** (Admin)
2. **Navega para "Gerenciar Usuários"** (menu lateral)
3. **Clica em "Adicionar Usuário"**
4. **Preenche formulário:**
   - Nome: "João Silva"
   - Email: "joao@empresademo.com"
   - Perfil: "Operador"
   - Senha temporária: "Temp@123"
5. **Sistema cria o usuário**
6. **João recebe email** com credenciais
7. **João faz login** e é forçado a trocar a senha
8. **João já pode usar o sistema** com permissões de Operador

---

## 📊 Métricas de Sucesso

- ✅ Cada membro da equipe tem seu próprio login
- ✅ Zero compartilhamento de senhas
- ✅ 100% das ações rastreáveis por usuário
- ✅ Admin pode desativar usuários de ex-funcionários
- ✅ Permissões respeitadas no frontend e backend

---

## ⏱️ Estimativa de Tempo

| Fase | Tempo Estimado |
|------|---------------|
| **Fase 1 - MVP** | 6-8 horas |
| Backend (Repository, Service, Controller) | 2-3 horas |
| Frontend (Telas de gerenciamento) | 3-4 horas |
| Testes e ajustes | 1 hora |
| **Fase 2 - Convites** | 4-5 horas |
| **Fase 3 - Auditoria** | 6-8 horas |
| **TOTAL COMPLETO** | 16-21 horas |

---

## 🎯 Decisões Necessárias

Antes de começar a implementação, preciso saber:

### ❓ **Questão 1: Qual fase implementar primeiro?**
- [ ] **Opção A:** Apenas Fase 1 (MVP - gerenciamento básico)
- [ ] **Opção B:** Fase 1 + Fase 2 (MVP + Convites)
- [ ] **Opção C:** Tudo (Fase 1 + 2 + 3)

### ❓ **Questão 2: Como criar novos usuários?**
- [ ] **Opção A:** Admin cria com senha temporária (mais simples)
- [ ] **Opção B:** Sistema de convites por email (mais profissional)

### ❓ **Questão 3: Auditoria é prioridade?**
- [ ] **Sim:** Implementar campos de auditoria desde o início
- [ ] **Não:** Focar primeiro no gerenciamento de usuários

### ❓ **Questão 4: Perfis são suficientes?**
Os perfis atuais (Admin, Operador, Visualizador) atendem ou precisa de mais granularidade?

### ❓ **Questão 5: Primeiro usuário Admin**
Como será criado o primeiro usuário Admin quando uma empresa se cadastra?
- [ ] Automático no cadastro da empresa
- [ ] Manual via migration/seed
- [ ] Já existe sistema de cadastro?

---

## 💡 Recomendação

**Sugiro começar com:**
1. ✅ **Fase 1 (MVP)** - gerenciamento básico de usuários
2. ✅ **Opção A** - Admin cria com senha temporária
3. ✅ **Campos de auditoria básicos** - ao menos CriadoPor

**Motivo:** Implementação rápida (6-8 horas) que já resolve 80% do problema. Depois podemos evoluir para convites e auditoria completa.

---

**🤔 O que você acha dessa proposta? Quer que eu comece implementando a Fase 1 (MVP)?**

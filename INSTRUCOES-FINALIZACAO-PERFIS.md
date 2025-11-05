# Instruções para Finalizar Sistema de Perfis

## ✅ O que já está implementado:

### Backend (100%):
- ✅ Removido perfil Visualizador do enum
- ✅ Regras de negócio implementadas
  - Proprietário pode tudo
  - Admin não pode editar/excluir outros Admins (apenas Proprietário pode)
  - Admin pode excluir Operadores
- ✅ Validações no service com currentUserId
- ✅ Controller atualizado

### Frontend (80%):
- ✅ Removido Visualizador do enum
- ✅ Form atualizado
- ✅ Menu: Operador vê apenas "Regras de Cobrança"
- ✅ Lista de usuários atualizada

## 🔧 O que falta (20%):

### 1. Desabilitar edição/exclusão em Regras de Cobrança para Operador

Arquivo: `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.ts`

Adicionar no componente:

```typescript
import { AuthService } from '../../../core/services/auth.service';

export class RegrasListComponent implements OnInit {
  // ... código existente

  isOperador = false;

  constructor(
    // ... outros services
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    // ... código existente

    // Verificar se é operador
    this.authService.currentUser$.subscribe(user => {
      this.isOperador = user?.perfil === 'Operador';
    });
  }

  // Método helper
  canEdit(): boolean {
    return !this.isOperador;
  }

  canDelete(): boolean {
    return !this.isOperador;
  }
}
```

Arquivo HTML: `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.html`

Nos botões de ação, adicionar `[disabled]="!canEdit()"` e `[disabled]="!canDelete()"`

### 2. Compilar e Testar

```bash
# Parar backend atual
# Ctrl+C no terminal ou matar processo

# Compilar backend
cd src/Cobrio.API
dotnet build

# Rodar backend
dotnet run

# Compilar frontend
cd cobrio-web
npm run build

# Rodar frontend dev
ng serve
```

### 3. Executar SQL para marcar proprietário

```sql
USE Cobrio;

UPDATE UsuarioEmpresa
SET EhProprietario = 1
WHERE Email = 'admin@empresademo.com.br';
```

## 🧪 Testes a Realizar:

### Como Admin:
1. ✅ Ver todos os menus
2. ✅ Criar usuário Operador
3. ✅ Não conseguir editar outro Admin
4. ✅ Não conseguir excluir outro Admin
5. ✅ Conseguir excluir Operador

### Como Operador:
1. ✅ Ver apenas "Regras de Cobrança" no menu
2. ✅ Não ver botões de editar/excluir em Regras de Cobrança
3. ✅ Apenas visualizar as regras

### Como Proprietário:
1. ✅ Ver todos os menus
2. ✅ Conseguir editar qualquer usuário
3. ✅ Conseguir excluir qualquer usuário
4. ✅ Badge "Proprietário" visível na lista
5. ✅ Proprietário não pode ser editado por outros
6. ✅ Proprietário não pode ser excluído

## 📋 Regras Finais Implementadas:

| Ação | Proprietário | Admin | Operador |
|------|--------------|-------|----------|
| Ver todos menus | ✅ | ✅ | ❌ (só Regras) |
| Gerenciar usuários | ✅ | ✅* | ❌ |
| Editar Admin | ✅ | ❌ | ❌ |
| Excluir Admin | ✅ | ❌ | ❌ |
| Editar Operador | ✅ | ✅ | ❌ |
| Excluir Operador | ✅ | ✅ | ❌ |
| Editar Regras | ✅ | ✅ | ❌ |
| Excluir Regras | ✅ | ✅ | ❌ |

*Admin não pode gerenciar outros Admins

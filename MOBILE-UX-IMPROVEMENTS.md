# Melhorias de UX/UI Mobile - Cobrio

## Visão Geral

Este documento descreve as melhorias de responsividade e UX mobile implementadas no sistema Cobrio, seguindo boas práticas de design mobile-first e diretrizes de acessibilidade.

## 🎯 Objetivos

- Melhorar a experiência de navegação em dispositivos móveis
- Garantir touch targets mínimos de 44x44px (Apple HIG & Material Design)
- Implementar padrões consistentes de responsividade
- Facilitar a manutenção com arquitetura SCSS organizada
- Manter 100% de compatibilidade com funcionalidades existentes

## 📁 Arquitetura de Estilos

### Novos Arquivos Criados

```
cobrio-web/src/styles/
├── _variables.scss          # Variáveis globais (cores, espaçamentos, breakpoints)
├── _mixins.scss            # Mixins reutilizáveis (responsive, flexbox, etc)
└── _responsive-utilities.scss # Utilitários responsivos (PrimeNG overrides, classes)
```

### Estrutura Organizada

- **Variáveis centralizadas**: Cores, breakpoints, z-index, transições, etc.
- **Mixins reutilizáveis**: Reduz duplicação de código
- **Mobile-first approach**: Estilos base para mobile, desktop como incremento

## 🔧 Principais Melhorias Implementadas

### 1. **Sidebar Mobile Aprimorado**

**Antes:**
- Height hardcoded (64px) não coincidia com header (90px)
- Sem backdrop overlay
- Sidebar não fechava após navegação
- Sem animações suaves

**Depois:**
- ✅ Height correto usando variáveis SCSS
- ✅ Backdrop overlay com fade animation
- ✅ Fecha automaticamente após navegação em mobile
- ✅ Animação slide-in suave
- ✅ Previne scroll do body quando sidebar aberta
- ✅ Fecha ao pressionar ESC
- ✅ Touch targets mínimos de 44px

**Código:**
```typescript
// main-layout.component.ts
toggleSidebar(): void {
  this.sidebarCollapsed = !this.sidebarCollapsed;
  if (!this.sidebarCollapsed && window.innerWidth < 768) {
    document.body.style.overflow = 'hidden'; // Previne scroll
  } else {
    document.body.style.overflow = '';
  }
}
```

### 2. **Header Responsivo**

**Melhorias:**
- ✅ Logo redimensiona automaticamente (550px → 300px → 200px)
- ✅ User info oculto em mobile (mantém só avatar)
- ✅ Notification panel full-width em mobile
- ✅ Dropdown menus adaptam posição
- ✅ Touch targets aumentados

**Breakpoints:**
- Desktop: 90px height
- Mobile: 70px height
- Micro (<480px): 50px logo height

### 3. **PrimeNG Components Otimizados**

#### Dialogs
```scss
.p-dialog {
  @include mobile-only {
    width: calc(100vw - 1rem) !important;
    max-height: 90vh !important;

    .p-dialog-footer button {
      width: 100%; // Botões full-width
      @include touch-target; // Mínimo 44px
    }
  }
}
```

#### Tables
```scss
.p-datatable {
  @include mobile-only {
    font-size: 0.875rem;

    .hide-mobile {
      display: none !important; // Esconde colunas menos importantes
    }
  }
}
```

#### Buttons
```scss
.p-button {
  @include mobile-only {
    @include touch-target; // 44x44px mínimo
    padding: 0.75rem 1.25rem;
  }
}
```

### 4. **Sistema de Grid Responsivo**

```html
<!-- Exemplo de uso -->
<div class="form-grid cols-3">
  <div class="form-field">
    <label>Nome</label>
    <input pInputText />
  </div>
  <!-- Desktop: 3 colunas, Tablet: 2 colunas, Mobile: 1 coluna -->
</div>
```

### 5. **View Alternativa de Tabelas (Card-based)**

Para tabelas com muitas colunas, implementamos uma view de cards em mobile:

```html
<!-- Desktop: Tabela tradicional -->
<p-table [value]="data" class="hide-on-mobile">
  <!-- Colunas -->
</p-table>

<!-- Mobile: Card view -->
<div class="table-card-view show-on-mobile">
  <div class="table-card" *ngFor="let item of data">
    <div class="card-row">
      <span class="card-label">Nome</span>
      <span class="card-value">{{ item.nome }}</span>
    </div>
    <!-- Mais campos -->
  </div>
</div>
```

## 📐 Breakpoints Padronizados

```scss
$breakpoint-xs: 480px;   // Celulares pequenos
$breakpoint-sm: 640px;   // Celulares
$breakpoint-md: 768px;   // Tablets portrait
$breakpoint-lg: 1024px;  // Tablets landscape / Desktop pequeno
$breakpoint-xl: 1280px;  // Desktop
$breakpoint-2xl: 1536px; // Desktop grande
```

## 🎨 Mixins Disponíveis

### Responsive
```scss
@include mobile-only { ... }        // max-width: 767px
@include tablet-and-up { ... }      // min-width: 768px
@include desktop-only { ... }       // min-width: 1024px
@include respond-above($bp) { ... } // Custom min-width
@include respond-below($bp) { ... } // Custom max-width
```

### Layout
```scss
@include flex-center { ... }        // display: flex + center
@include flex-between { ... }       // display: flex + space-between
@include flex-column { ... }        // display: flex + column
```

### Typography
```scss
@include truncate { ... }           // Trunca texto com ellipsis
@include line-clamp(2) { ... }     // Limita a N linhas
```

### Touch & Accessibility
```scss
@include touch-target($size) { ... } // Mínimo 44x44px
@include focus-visible { ... }       // Outline acessível
@include hover-state { ... }         // Apenas em dispositivos com hover
```

### Utilities
```scss
@include custom-scrollbar { ... }    // Scrollbar customizada
@include backdrop-overlay { ... }    // Overlay com fade
@include card-shadow { ... }         // Shadow com hover
@include safe-area-padding { ... }   // Para dispositivos com notch
```

## 🛠️ Classes Utilitárias

### Visibilidade
```html
<div class="hide-on-mobile">Visível apenas em desktop</div>
<div class="show-on-mobile">Visível apenas em mobile</div>
```

### Layout
```html
<div class="stack-on-mobile">Stack vertical em mobile</div>
<div class="full-width-mobile">Full width em mobile</div>
```

### Spacing
```html
<section class="section-spacing">Padding responsivo</section>
<div class="card-spacing">Padding de card responsivo</div>
```

### Container
```html
<div class="container">Max-width 1280px centralizado</div>
<div class="container-fluid">Full width com padding</div>
```

## 🔍 Detalhes Técnicos

### Z-Index Scale
```scss
$z-sticky: 100;           // Header fixo
$z-sidebar-mobile: 998;   // Sidebar em mobile
$z-modal-backdrop: 999;   // Backdrop de modais
$z-dropdown: 1000;        // Dropdowns
$z-modal: 1001;          // Modais
$z-notification: 1002;    // Notificações
```

### Transições
```scss
$transition-fast: 0.15s ease-in-out;
$transition-base: 0.2s ease-in-out;
$transition-slow: 0.3s ease-in-out;
```

### Touch Targets
- Mínimo: **44x44px** (recomendação Apple HIG e Material Design)
- Aplicado em: Botões, links de navegação, ícones clicáveis, inputs

### Safe Area (Notch)
```scss
@include safe-area-top;     // padding-top: env(safe-area-inset-top)
@include safe-area-bottom;  // padding-bottom: env(safe-area-inset-bottom)
@include safe-area-padding; // left + right
```

## 📱 Comportamentos Mobile

### Sidebar
- **Desktop**: Visível por padrão, colapsável para ícones
- **Mobile**: Oculto por padrão, overlay full quando aberto
- **Interações**:
  - Abre/fecha com botão hamburger
  - Fecha ao clicar fora (backdrop)
  - Fecha após navegação
  - Fecha com tecla ESC
  - Previne scroll do body quando aberto

### Header
- **Desktop**: 90px, mostra nome/email do usuário
- **Mobile**: 70px, esconde informações do usuário
- **Logo**: Redimensiona automaticamente
- **Notificações**: Painel adapta largura à tela

### Dialogs
- **Desktop**: Largura fixa (ex: 700px)
- **Mobile**: calc(100vw - 1rem), max-height 90vh
- **Botões**: Full width em mobile (vertical stack)

### Forms
- **Desktop**: Grid multi-coluna
- **Mobile**: Single column, spacing otimizado
- **Inputs**: Font-size 16px (previne zoom no iOS)

## 🎯 Próximos Passos (Opcional)

### Melhorias Futuras Sugeridas
1. **Swipe Gestures**: Adicionar HammerJS para gestos de swipe na sidebar
2. **Bottom Navigation**: Menu inferior para ações rápidas em mobile
3. **Pull to Refresh**: Atualizar listas com gesto de puxar
4. **Infinite Scroll**: Substituir paginação em mobile
5. **PWA**: Transformar em Progressive Web App
6. **Dark Mode**: Implementar tema escuro

## 📚 Guia de Uso para Desenvolvedores

### Como Usar os Mixins em Componentes

```scss
// component.scss
@import '../../../styles/variables';
@import '../../../styles/mixins';

.my-component {
  padding: $spacing-lg;

  @include mobile-only {
    padding: $spacing-md;
  }

  .button {
    @include touch-target;
    @include hover-state {
      background: $primary-color;
    }
  }
}
```

### Como Criar Tabelas Responsivas

**Opção 1: Wrapper scrollável**
```html
<div class="table-responsive-wrapper">
  <p-table [value]="data">
    <ng-template pTemplate="header">
      <tr>
        <th>Sempre visível</th>
        <th class="hide-mobile">Oculto em mobile</th>
      </tr>
    </ng-template>
  </p-table>
</div>
```

**Opção 2: Card view alternativa**
```html
<!-- Tabela desktop -->
<p-table [value]="data" class="hide-on-mobile">...</p-table>

<!-- Cards mobile -->
<div class="table-card-view show-on-mobile">
  <div class="table-card" *ngFor="let item of data">
    <div class="card-row">
      <span class="card-label">Nome</span>
      <span class="card-value">{{ item.nome }}</span>
    </div>
  </div>
</div>
```

### Como Criar Forms Responsivos

```html
<div class="form-grid cols-2">
  <div class="form-field">
    <label>Campo 1</label>
    <input pInputText />
    <small>Dica</small>
  </div>
  <div class="form-field">
    <label>Campo 2</label>
    <input pInputText />
  </div>
</div>
```

## ✅ Checklist de Responsividade

Ao criar novos componentes, verifique:

- [ ] Touch targets mínimos de 44x44px
- [ ] Fonte mínima de 16px em inputs (previne zoom iOS)
- [ ] Espaçamento adequado entre elementos clicáveis (mínimo 8px)
- [ ] Testado em viewport 320px (iPhone SE)
- [ ] Testado em viewport 375px (iPhone padrão)
- [ ] Testado em viewport 768px (iPad portrait)
- [ ] Testado em viewport 1024px (iPad landscape)
- [ ] Navegação por teclado funciona
- [ ] Estados de focus visíveis
- [ ] Sem scroll horizontal indesejado
- [ ] Imagens responsivas (srcset ou object-fit)
- [ ] Textos legíveis sem zoom
- [ ] Botões acessíveis por toque

## 🐛 Troubleshooting

### Problema: SCSS não compila
**Solução**: Verifique se os imports estão corretos em `styles.scss`:
```scss
@import "styles/variables";
@import "styles/mixins";
@import "styles/responsive-utilities";
```

### Problema: Sidebar não fecha em mobile
**Solução**: Verifique se o evento `(closeSidebar)` está conectado:
```html
<app-sidebar
  [collapsed]="sidebarCollapsed"
  (closeSidebar)="closeSidebar()">
</app-sidebar>
```

### Problema: Backdrop não aparece
**Solução**: Verifique a lógica de `*ngIf`:
```html
<div class="sidebar-backdrop"
     *ngIf="!sidebarCollapsed"
     [@fadeAnimation]>
</div>
```

## 📖 Referências

- [Apple Human Interface Guidelines - Touch Targets](https://developer.apple.com/design/human-interface-guidelines/inputs/touchscreen-gestures)
- [Material Design - Touch Targets](https://material.io/design/usability/accessibility.html#layout-and-typography)
- [PrimeNG Documentation](https://primeng.org/)
- [Tailwind CSS Documentation](https://tailwindcss.com/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

---

**Data de Implementação**: Novembro 2025
**Autor**: Claude Code Assistant
**Versão**: 1.0

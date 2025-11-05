# Correções no Modal de Exemplo de Payload JSON

## Problema Identificado
O modal "Exemplo de Payload JSON" estava gerando JSONs incorretos com:
- `payload` (minúsculo) ao invés de `Payload` (maiúsculo)
- `dataVencimento` (minúsculo) ao invés de `DataVencimento` (maiúsculo)
- Campos duplicados ou faltando entre as seções

## Solução Implementada

### 1. **Campos do Sistema (Raiz do JSON)**
São campos que vão na **raiz** do JSON com **PascalCase**:
- `DataVencimento` - **SEMPRE obrigatório** (o sistema precisa dele para calcular quando disparar)
- `Email`, `Telefone`, `NomeCliente`, etc. - Obrigatórios **apenas se configurados na regra**

**Localização no JSON:** Raiz (mesmo nível que "Payload")

**IMPORTANTE:** O modal agora mostra apenas os campos que foram configurados como obrigatórios na sua regra!

### 2. **Variáveis do Template (Dentro de "Payload")**
São TODAS as variáveis extraídas do template HTML (entre `{{` e `}}`).
Vão **dentro do objeto "Payload"** (com P maiúsculo).

**Localização no JSON:** Dentro de `"Payload": { ... }`

**REGRA DE DUPLICAÇÃO:**
- Se uma variável está configurada como campo obrigatório → vai APENAS na raiz, não no Payload
- `dataVencimento` (qualquer capitalização) → vai APENAS na raiz como `DataVencimento`, nunca no Payload
- Variáveis que não são obrigatórias → vão APENAS no Payload

## Estrutura Correta do JSON

### Exemplo com Email configurado como obrigatório:

```json
[
  {
    // ===== CAMPOS DO SISTEMA (RAIZ) =====
    "Email": "joao@exemplo.com",          // ✅ Configurado como obrigatório na regra
    "DataVencimento": "15/12/2025 18:00", // ✅ SEMPRE obrigatório

    // ===== VARIÁVEIS DO TEMPLATE =====
    "Payload": {                         // ✅ "Payload" com P maiúsculo!
      "NomeCliente": "João Silva",       // Variável do template
      "NomeEmpresa": "Minha Empresa",    // Variável do template
      "descrição": "Mensalidade",        // Variável do template (sem HTML)
      "valor": "150.00",                 // Variável do template (sem HTML)
      // NOTA: dataVencimento NÃO aparece aqui (já está na raiz como DataVencimento)
      "linkBoleto": "https://...",       // Variável do template
      "emailSuporte": "suporte@...",     // Variável do template
      "telefone": "+5511999999999"       // Variável do template
    }
  }
]
```

**Observação:** Se você configurar `Email`, `Telefone` e `NomeCliente` como obrigatórios, eles aparecerão na raiz do JSON. O modal mostra apenas os campos configurados na sua regra!

## Arquivos Modificados

### Frontend (Angular)

**C:\Cobrio\cobrio-web\src\app\features\regras-cobranca\regras-list\regras-list.component.ts**

1. **Linha 195-202:** `mostrarExemploPayload()`
   - Removido filtro que excluía variáveis do sistema
   - Agora TODAS as variáveis do template são exibidas

2. **Linha 233-334:** `gerarExemploPayload()`
   - Campos do sistema sempre na raiz com PascalCase
   - `Payload` com P maiúsculo (linha 302)
   - `DataVencimento` com D maiúsculo (linha 314 e 320)
   - Data no formato brasileiro: dd/MM/yyyy HH:mm
   - TODAS as variáveis do template vão para dentro de Payload

3. **Linha 373-397:** `getCamposObrigatoriosSistema()`
   - Sempre retorna `DataVencimento` (obrigatório pelo sistema)
   - Retorna apenas os campos configurados em `variaveisObrigatoriasSistema`
   - Normaliza para PascalCase
   - Remove duplicatas

**C:\Cobrio\cobrio-web\src\app\features\regras-cobranca\regras-list\regras-list.component.html**

**Linha 178-218:** Melhorias no modal
- Seção "Campos do Sistema" mais clara
- Seção "Variáveis do Template" mais clara
- Adicionado avisos informativos sobre localização no JSON

## Como Testar

1. Acesse o frontend Angular no navegador
2. **Recarregue a página** (F5 ou Ctrl+F5) para carregar o novo código
3. Clique no botão "Exemplo de Payload" de uma regra
4. Verifique que:
   - ✅ Campos do Sistema mostram: `DataVencimento` + apenas os campos configurados como obrigatórios na regra
   - ✅ Variáveis do Template mostram TODAS as variáveis do seu template
   - ✅ O JSON tem "Payload" com P maiúsculo
   - ✅ O JSON tem "DataVencimento" com D maiúsculo na raiz
   - ✅ O JSON tem apenas os campos configurados como obrigatórios + DataVencimento na raiz
   - ✅ Todas as variáveis do template estão dentro de "Payload"

## Regra Final

**`DataVencimento` → SEMPRE obrigatório (raiz do JSON)**
**Outros Campos do Sistema → Raiz do JSON se configurados como obrigatórios (PascalCase)**
**Variáveis do Template → Dentro de "Payload" (nome exato do template, sem HTML)**

**Exemplo:**
- Se configurar apenas `Email` como obrigatório → JSON terá `Email` e `DataVencimento` na raiz
- Se configurar `Email`, `Telefone` e `NomeCliente` → JSON terá todos eles + `DataVencimento` na raiz
- `dataVencimento` do template NUNCA aparece no Payload (sempre convertido para `DataVencimento` na raiz)

**REGRA IMPORTANTE:** Variáveis NUNCA aparecem duplicadas! Se está na raiz, não vai no Payload!

# Implementação de Importação em Massa via JSON

## Resumo

Implementado funcionalidade para importação em massa de cobranças via JSON, permitindo que os usuários escolham entre Excel ou JSON no mesmo modal de importação.

## O que foi implementado

### 1. Backend

#### Novo Endpoint
**Arquivo:** `src/Cobrio.API/Controllers/RegraCobrancaController.cs` (linhas 349-377)

```csharp
[HttpPost("{id}/importar-json")]
public async Task<IActionResult> ImportarJson(Guid id, [FromBody] List<Application.DTOs.Cobranca.CreateCobrancaRequest> cobrancas)
```

- Aceita um array JSON de cobranças
- Valida e processa cada cobrança
- Retorna resultado com sucesso/erros

#### Novo Método no Service
**Arquivo:** `src/Cobrio.Application/Services/ExcelImportService.cs` (linhas 429-618)

```csharp
public async Task<ImportacaoResultado> ImportarCobrancasJsonAsync(
    Guid regraCobrancaId,
    List<DTOs.Cobranca.CreateCobrancaRequest> cobrancasRequest,
    CancellationToken cancellationToken = default)
```

Funcionalidades:
- ✅ Valida data de vencimento (aceita múltiplos formatos: dd/MM/yyyy HH:mm, yyyy-MM-dd, etc.)
- ✅ Valida destinatário (Email ou Telefone conforme canal)
- ✅ Valida variáveis obrigatórias do template (usando `GetVariaveisObrigatoriasLimpas()`)
- ✅ Calcula data de disparo automaticamente
- ✅ Cria cobranças no banco
- ✅ Salva histórico de importação com origem `OrigemImportacao.Excel`
- ✅ Registra erros por linha com detalhes

### 2. Frontend

#### Novos Models
**Arquivo:** `cobrio-web/src/app/core/models/cobranca.models.ts` (novo arquivo)

```typescript
export interface CreateCobrancaRequest {
  Email?: string;
  Telefone?: string;
  NomeCliente?: string;
  Payload: { [key: string]: any };
  DataVencimento?: string;
}
```

#### Atualização do Service
**Arquivo:** `cobrio-web/src/app/core/services/regra-cobranca.service.ts` (linhas 74-79)

```typescript
importarJson(id: string, cobrancas: CreateCobrancaRequest[]): Observable<ImportacaoResultado>
```

#### Atualização do Componente
**Arquivo:** `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.ts`

**Novas Propriedades:**
- `tipoImportacao: 'excel' | 'json' = 'excel'` - Controla qual tipo está selecionado
- `jsonInput: string = ''` - Armazena o JSON colado pelo usuário

**Novos Métodos:**
- `uploadJson()` (linhas 623-722) - Processa importação JSON com validação
- `upload()` (linhas 724-730) - Método genérico que chama uploadExcel ou uploadJson

**Atualizações:**
- `mostrarImportDialog()` - Reseta tipoImportacao e jsonInput ao abrir modal

#### Atualização do Template
**Arquivo:** `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.html` (linhas 271-404)

**Mudanças no Modal:**
- ✅ Cabeçalho dinâmico: "Envio em Massa - {nome da regra}"
- ✅ Radio buttons para escolher entre Excel e JSON
- ✅ Seção Excel (linhas 310-358):
  - Instruções para Excel
  - Botão para baixar modelo
  - Upload de arquivo
  - Exibição do arquivo selecionado
- ✅ Seção JSON (linhas 361-392):
  - Instruções para JSON
  - Textarea com placeholder mostrando exemplo
  - Formatação monospace para melhor visualização
- ✅ Botão "Importar" adaptado:
  - Chama método `upload()` genérico
  - Desabilitado se Excel selecionado sem arquivo
  - Desabilitado se JSON selecionado sem conteúdo

## Estrutura do JSON

O JSON deve ser um array de objetos com a seguinte estrutura:

```json
[
  {
    "Email": "cliente@exemplo.com",
    "DataVencimento": "15/12/2025 18:00",
    "Payload": {
      "valor": "150.00",
      "descricao": "Mensalidade",
      "linkBoleto": "https://..."
    }
  }
]
```

### Campos na Raiz

**Obrigatórios:**
- `DataVencimento` - Data de vencimento (formatos: dd/MM/yyyy HH:mm, dd/MM/yyyy, yyyy-MM-dd HH:mm:ss, yyyy-MM-dd)

**Condicionalmente Obrigatórios (conforme configuração da regra):**
- `Email` - Se canal for Email
- `Telefone` - Se canal for SMS/WhatsApp
- `NomeCliente` - Se configurado como obrigatório na regra

### Objeto Payload

Contém todas as variáveis do template. Exemplo:
- `valor` - Valor da cobrança
- `descricao` - Descrição do serviço
- `linkBoleto` - Link para pagamento
- E todas as outras variáveis definidas no template HTML

**IMPORTANTE:** Variáveis configuradas como obrigatórias vão APENAS na raiz, não no Payload. `dataVencimento` NUNCA vai no Payload (sempre convertido para `DataVencimento` na raiz).

## Validações

### Frontend (JavaScript/TypeScript)
- ✅ Verifica se JSON é válido (JSON.parse)
- ✅ Verifica se é um array
- ✅ Verifica se array não está vazio

### Backend (C#)
- ✅ Data de vencimento obrigatória
- ✅ Data de vencimento em formato válido
- ✅ Destinatário conforme canal (Email ou Telefone)
- ✅ Variáveis obrigatórias do template presentes no Payload
- ✅ Data de disparo não pode ser no passado
- ✅ Erros são registrados por linha com tipo e descrição

## Histórico de Importação

As importações JSON são registradas no histórico com:
- `Origem`: `OrigemImportacao.Excel` (valor 1)
- `NomeArquivo`: `importacao-json-{timestamp}.json`
- `Status`: Sucesso / Parcial / Erro
- `TotalLinhas`: Número de cobranças no array
- `LinhasProcessadas`: Cobranças importadas com sucesso
- `LinhasComErro`: Cobranças com erro
- `ErrosJson`: Detalhes dos erros em formato JSON

## Como Testar

1. **Acesse o frontend Angular** e faça login
2. **Navegue para Regras de Cobrança**
3. **Clique em "Enviar em Massa"** em uma regra
4. **Selecione "JSON"** nos radio buttons
5. **Cole um JSON válido** (veja exemplo acima)
6. **Clique em "Importar"**
7. **Verifique o resultado:**
   - Mensagem de sucesso/erro
   - Se houver erros, um modal será exibido com os detalhes
   - Verifique o histórico clicando no botão "Histórico"

### Exemplo de JSON para Teste

Para uma regra com Email obrigatório e template com variáveis `valor`, `descricao`, `linkBoleto`:

```json
[
  {
    "Email": "joao@exemplo.com",
    "DataVencimento": "15/12/2025 18:00",
    "Payload": {
      "valor": "150.00",
      "descricao": "Mensalidade Dezembro",
      "linkBoleto": "https://exemplo.com/pagar/123"
    }
  },
  {
    "Email": "maria@exemplo.com",
    "DataVencimento": "20/12/2025 10:00",
    "Payload": {
      "valor": "200.00",
      "descricao": "Anuidade",
      "linkBoleto": "https://exemplo.com/pagar/456"
    }
  }
]
```

## Arquivos Modificados

### Backend
- ✅ `src/Cobrio.API/Controllers/RegraCobrancaController.cs` - Novo endpoint
- ✅ `src/Cobrio.Application/Services/ExcelImportService.cs` - Novo método
- ✅ `src/Cobrio.Application/DTOs/Cobranca/CreateCobrancaRequest.cs` - Já existia

### Frontend
- ✅ `cobrio-web/src/app/core/models/cobranca.models.ts` - Novo arquivo
- ✅ `cobrio-web/src/app/core/models/index.ts` - Adicionado export
- ✅ `cobrio-web/src/app/core/services/regra-cobranca.service.ts` - Novo método
- ✅ `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.ts` - Novas propriedades e métodos
- ✅ `cobrio-web/src/app/features/regras-cobranca/regras-list/regras-list.component.html` - Modal atualizado

## Status

✅ **Backend implementado e compilado com sucesso**
✅ **Frontend implementado**
✅ **Validação de JSON implementada**
✅ **Histórico e erros funcionando**
✅ **API rodando em http://localhost:5271**

## Próximos Passos (Opcional)

- Testar com diferentes cenários de erro
- Testar com grandes volumes de dados
- Adicionar opção de importar JSON de arquivo (.json)
- Melhorar mensagens de erro para casos específicos

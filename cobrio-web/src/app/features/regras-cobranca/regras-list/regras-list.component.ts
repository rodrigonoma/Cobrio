import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { RegraCobrancaService } from '../../../core/services/regra-cobranca.service';
import { RegraCobranca, HistoricoImportacao, StatusImportacao, OrigemImportacao } from '../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';

@Component({
  selector: 'app-regras-list',
  templateUrl: './regras-list.component.html',
  styleUrls: ['./regras-list.component.scss']
})
export class RegrasListComponent implements OnInit {
  regras: RegraCobranca[] = [];
  loading = true;

  // Permissões dinâmicas
  perfilUsuarioString: string = 'Admin';
  podeVisualizar = false;
  podeCriar = false;
  podeEditar = false;
  podeExcluir = false;
  podeExportar = false;
  podeImportar = false;

  displayUrlDialog = false;
  displayPayloadDialog = false;
  displayImportDialog = false;
  displayHistoricoDialog = false;
  selectedRegra: RegraCobranca | null = null;
  payloadVariaveis: string[] = [];
  exemploPayloadJson: string = '';
  exemploCurl: string = '';
  selectedFile: File | null = null;
  uploadProgress = false;
  errosValidacao: any[] = [];
  displayErrosDialog = false;
  historicoImportacoes: HistoricoImportacao[] = [];
  loadingHistorico = false;
  selectedHistorico: HistoricoImportacao | null = null;
  displayDetalhesHistoricoDialog = false;
  StatusImportacao = StatusImportacao;
  OrigemImportacao = OrigemImportacao;

  // Propriedades para importação JSON
  tipoImportacao: 'excel' | 'json' = 'excel';
  jsonInput: string = '';

  // Propriedades para filtros do histórico
  filtros = {
    dataInicio: null as Date | null,
    dataFim: null as Date | null,
    origem: null as number | null,
    usuario: null as string | null,
    status: null as number | null
  };
  historicoFiltrado: HistoricoImportacao[] = [];
  usuariosUnicos: string[] = [];
  mostrarFiltros = false;

  @ViewChild('fileUpload') fileUpload!: FileUpload;

  constructor(
    private regraService: RegraCobrancaService,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private authService: AuthService,
    private permissaoService: PermissaoService
  ) { }

  ngOnInit(): void {
    // Carregar perfil do usuário e suas permissões
    this.authService.currentUser$.subscribe(user => {
      if (user?.perfil) {
        this.perfilUsuarioString = user.perfil;
        this.carregarPermissoes();
      }
    });

    this.loadRegras();
  }

  carregarPermissoes(): void {
    const moduloChave = 'regras-cobranca';

    // Verificar permissão de visualizar detalhes (read) - Controla botão de ver detalhes
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'read').subscribe({
      next: (result) => {
        this.podeVisualizar = result.permitido;
      },
      error: (err) => console.error('Erro ao verificar permissão de visualização:', err)
    });

    // Verificar permissão de criar (create)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'create').subscribe({
      next: (result) => this.podeCriar = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de criação:', err)
    });

    // Verificar permissão de editar (update)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'update').subscribe({
      next: (result) => this.podeEditar = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de edição:', err)
    });

    // Verificar permissão de excluir (delete)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'delete').subscribe({
      next: (result) => this.podeExcluir = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de exclusão:', err)
    });

    // Verificar permissão de exportar (export)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'export').subscribe({
      next: (result) => this.podeExportar = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de exportação:', err)
    });

    // Verificar permissão de importar (import)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'import').subscribe({
      next: (result) => this.podeImportar = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de importação:', err)
    });
  }

  // Métodos helper para verificar permissões
  canEdit(): boolean {
    return this.podeEditar;
  }

  canDelete(): boolean {
    return this.podeExcluir;
  }

  canCreate(): boolean {
    return this.podeCriar;
  }

  canExport(): boolean {
    return this.podeExportar;
  }

  canImport(): boolean {
    return this.podeImportar;
  }

  loadRegras(): void {
    this.loading = true;
    this.regraService.getAll().subscribe({
      next: (regras) => {
        this.regras = regras;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar regras:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar regras de cobrança'
        });
        this.loading = false;
      }
    });
  }

  novaRegra(): void {
    this.router.navigate(['/regras-cobranca/novo']);
  }

  editarRegra(regra: RegraCobranca): void {
    this.router.navigate(['/regras-cobranca/editar', regra.id]);
  }

  excluirRegra(regra: RegraCobranca): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a regra "${regra.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.regraService.delete(regra.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Regra excluída com sucesso'
            });
            this.loadRegras();
          },
          error: (error) => {
            console.error('Erro ao excluir regra:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao excluir regra'
            });
          }
        });
      }
    });
  }

  toggleAtivo(regra: RegraCobranca): void {
    const action = regra.ativa ? 'desativar' : 'ativar';
    const service = regra.ativa
      ? this.regraService.desativar(regra.id)
      : this.regraService.ativar(regra.id);

    service.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `Regra ${action === 'ativar' ? 'ativada' : 'desativada'} com sucesso`
        });
        this.loadRegras();
      },
      error: (error) => {
        console.error(`Erro ao ${action} regra:`, error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: `Erro ao ${action} regra`
        });
      }
    });
  }

  regenerarToken(regra: RegraCobranca): void {
    this.confirmationService.confirm({
      message: 'Tem certeza que deseja regenerar o token? A URL antiga deixará de funcionar.',
      header: 'Confirmar Regeneração',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.regraService.regenerarToken(regra.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Token regenerado com sucesso'
            });
            this.loadRegras();
          },
          error: (error) => {
            console.error('Erro ao regenerar token:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao regenerar token'
            });
          }
        });
      }
    });
  }

  mostrarUrl(regra: RegraCobranca): void {
    this.selectedRegra = regra;
    this.displayUrlDialog = true;
  }

  copiarUrl(url: string): void {
    navigator.clipboard.writeText(url).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado',
        detail: 'URL copiada para a área de transferência'
      });
    }).catch(() => {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro ao copiar URL'
      });
    });
  }

  getStatusClass(ativa: boolean): string {
    return ativa ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  }

  getStatusLabel(ativa: boolean): string {
    return ativa ? 'Ativa' : 'Inativa';
  }

  gerarWebhookUrl(token: string): string {
    const baseUrl = window.location.origin.replace('4201', '5271').replace('4200', '5271');
    return `${baseUrl}/api/webhook/${token}`;
  }

  mostrarExemploPayload(regra: RegraCobranca): void {
    this.selectedRegra = regra;

    // Extrair todas as variáveis do template
    const todasVariaveis = this.extrairVariaveis(regra.templateNotificacao);

    // Filtrar variáveis que já estão nos campos obrigatórios do sistema
    const camposObrigatorios = regra.variaveisObrigatoriasSistema
      ? JSON.parse(regra.variaveisObrigatoriasSistema).map((v: string) => v.toLowerCase())
      : [];

    // Sempre adicionar 'datavencimento' aos campos obrigatórios (pois DataVencimento sempre vai na raiz)
    camposObrigatorios.push('datavencimento');

    // payloadVariaveis = apenas as que NÃO estão nos campos obrigatórios
    this.payloadVariaveis = todasVariaveis.filter(v =>
      !camposObrigatorios.includes(v.toLowerCase())
    );

    this.gerarExemploPayload(regra);
    this.displayPayloadDialog = true;
  }

  extrairVariaveis(template: string): string[] {
    if (!template) return [];

    const regex = /\{\{([^}]+)\}\}/g;
    const variaveis = new Set<string>();
    let match;

    while ((match = regex.exec(template)) !== null) {
      // Remover tags HTML, entidades HTML e limpar espaços
      let variavel = match[1]
        .replace(/<\/?[^>]+(>|$)/g, '') // Remove todas as tags HTML (abertura e fechamento)
        .replace(/&nbsp;/g, ' ') // Remove &nbsp;
        .replace(/&[a-z]+;/gi, '') // Remove outras entidades HTML
        .replace(/\s+/g, ' ') // Normaliza múltiplos espaços
        .trim();

      if (variavel) {
        variaveis.add(variavel);
      }
    }

    return Array.from(variaveis).sort();
  }

  gerarExemploPayload(regra: RegraCobranca): void {
    const dataVencimento = new Date();
    dataVencimento.setDate(dataVencimento.getDate() + 15);

    // Montar objeto base - inicialmente vazio
    const payloadObj: any = {};

    // Adicionar apenas os campos obrigatórios do sistema configurados
    if (regra.variaveisObrigatoriasSistema) {
      try {
        const variaveisSistema = JSON.parse(regra.variaveisObrigatoriasSistema);
        variaveisSistema.forEach((variavel: string) => {
          const variavelLower = variavel.toLowerCase();
          const variavelNormalizada = variavel.charAt(0).toUpperCase() + variavel.slice(1);

          if (variavelLower.includes('nome') || variavelLower.includes('cliente')) {
            payloadObj[variavelNormalizada] = 'João Silva';
          } else if (variavelLower.includes('email')) {
            payloadObj[variavelNormalizada] = 'joao.silva@example.com';
          } else if (variavelLower.includes('telefone')) {
            payloadObj[variavelNormalizada] = '+5511999999999';
          } else if (variavelLower.includes('cpf')) {
            payloadObj[variavelNormalizada] = '12345678901';
          } else if (variavelLower.includes('cnpj')) {
            payloadObj[variavelNormalizada] = '12345678000190';
          } else {
            payloadObj[variavelNormalizada] = 'Valor de exemplo';
          }
        });
      } catch (e) {
        // Se falhar ao parsear, ignora
      }
    }

    // Adicionar variáveis do template no objeto Payload (com P maiúsculo)
    // payloadVariaveis já vem filtrado (sem os campos obrigatórios)
    const payloadVariaveisObj: any = {};

    this.payloadVariaveis.forEach(variavel => {
      // Valores de exemplo baseados no nome da variável
      if (variavel.toLowerCase().includes('valor')) {
        payloadVariaveisObj[variavel] = '150.00';
      } else if (variavel.toLowerCase().includes('data') || variavel.toLowerCase().includes('vencimento')) {
        payloadVariaveisObj[variavel] = '15/12/2025';
      } else if (variavel.toLowerCase().includes('link') || variavel.toLowerCase().includes('url')) {
        payloadVariaveisObj[variavel] = 'https://seusite.com/pagar/123456';
      } else if (variavel.toLowerCase().includes('codigo') || variavel.toLowerCase().includes('barras')) {
        payloadVariaveisObj[variavel] = '34191790010104351004791020150008291070026000';
      } else if (variavel.toLowerCase().includes('linha') || variavel.toLowerCase().includes('digitavel')) {
        payloadVariaveisObj[variavel] = '34191.79001 01043.510047 91020.150008 2 91070026000';
      } else if (variavel.toLowerCase().includes('numero') || variavel.toLowerCase().includes('fatura')) {
        payloadVariaveisObj[variavel] = 'FAT-2025-00123';
      } else if (variavel.toLowerCase().includes('empresa')) {
        payloadVariaveisObj[variavel] = 'Minha Empresa Ltda';
      } else if (variavel.toLowerCase().includes('nome') && variavel.toLowerCase().includes('cliente')) {
        payloadVariaveisObj[variavel] = 'João Silva';
      } else if (variavel.toLowerCase().includes('telefone')) {
        payloadVariaveisObj[variavel] = '+5511999999999';
      } else {
        payloadVariaveisObj[variavel] = 'Valor de exemplo';
      }
    });

    // IMPORTANTE: "Payload" com P maiúsculo!
    payloadObj.Payload = payloadVariaveisObj;

    // Adicionar data de vencimento no formato apropriado (IMPORTANTE: "DataVencimento" com D maiúsculo!)
    // Se unidade de tempo é Minuto (1) ou Hora (2), usar formato completo com hora
    // Se unidade de tempo é Dia (3), usar apenas data
    if (regra.unidadeTempo === 1 || regra.unidadeTempo === 2) {
      // Formato brasileiro: dd/MM/yyyy HH:mm
      const day = String(dataVencimento.getDate()).padStart(2, '0');
      const month = String(dataVencimento.getMonth() + 1).padStart(2, '0');
      const year = dataVencimento.getFullYear();
      const hours = String(dataVencimento.getHours()).padStart(2, '0');
      const minutes = String(dataVencimento.getMinutes()).padStart(2, '0');
      payloadObj.DataVencimento = `${day}/${month}/${year} ${hours}:${minutes}`;
    } else {
      // Formato brasileiro: dd/MM/yyyy
      const day = String(dataVencimento.getDate()).padStart(2, '0');
      const month = String(dataVencimento.getMonth() + 1).padStart(2, '0');
      const year = dataVencimento.getFullYear();
      payloadObj.DataVencimento = `${day}/${month}/${year}`;
    }

    // Criar array com o objeto (payload deve ser uma lista)
    const payloadArray = [payloadObj];

    // Gerar JSON formatado
    this.exemploPayloadJson = JSON.stringify(payloadArray, null, 2);

    // Gerar exemplo cURL
    const webhookUrl = this.gerarWebhookUrl(regra.tokenWebhook);
    this.exemploCurl = `curl -X POST "${webhookUrl}" \\
  -H "Content-Type: application/json; charset=utf-8" \\
  -d '${JSON.stringify(payloadArray, null, 2)}'`;
  }

  copiarPayloadExemplo(): void {
    navigator.clipboard.writeText(this.exemploPayloadJson).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado',
        detail: 'JSON copiado para a área de transferência'
      });
    }).catch(() => {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro ao copiar JSON'
      });
    });
  }

  copiarCurlExemplo(): void {
    navigator.clipboard.writeText(this.exemploCurl).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado',
        detail: 'Comando cURL copiado para a área de transferência'
      });
    }).catch(() => {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'Erro ao copiar cURL'
      });
    });
  }

  getCanalIcon(canalNotificacao: number): string {
    const icones: { [key: number]: string } = {
      1: 'pi pi-envelope',
      2: 'pi pi-comments',
      3: 'pi pi-whatsapp'
    };
    return icones[canalNotificacao] || 'pi pi-bell';
  }

  getCanalLabel(canalNotificacao: number): string {
    const canais: { [key: number]: string } = {
      1: 'Email',
      2: 'SMS',
      3: 'WhatsApp'
    };
    return canais[canalNotificacao] || 'Desconhecido';
  }

  getMomentoLabel(regra: RegraCobranca): string {
    if (regra.ehPadrao) {
      return 'Imediato';
    }
    const tipo = regra.tipoMomento === 1 ? 'Antes' : regra.tipoMomento === 2 ? 'Depois' : 'Exatamente';
    const unidade = regra.unidadeTempo === 1 ? 'minuto(s)' : regra.unidadeTempo === 2 ? 'hora(s)' : 'dia(s)';
    return `${regra.valorTempo} ${unidade} ${tipo} do vencimento`;
  }

  getCamposObrigatoriosSistema(regra: RegraCobranca): string[] {
    const campos: string[] = [];

    // DataVencimento é SEMPRE obrigatório (sistema precisa dele)
    campos.push('DataVencimento');

    // Adicionar apenas os campos configurados em variaveisObrigatoriasSistema
    if (regra.variaveisObrigatoriasSistema) {
      try {
        const variaveisSistema = JSON.parse(regra.variaveisObrigatoriasSistema);
        variaveisSistema.forEach((variavel: string) => {
          // Normalizar para PascalCase
          const variavelNormalizada = variavel.charAt(0).toUpperCase() + variavel.slice(1);
          if (!campos.includes(variavelNormalizada)) {
            campos.push(variavelNormalizada);
          }
        });
      } catch (e) {
        // Se falhar ao parsear, ignora
      }
    }

    // Remover duplicatas e ordenar
    return [...new Set(campos)].sort();
  }

  mostrarImportDialog(regra: RegraCobranca): void {
    if (!this.podeImportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para importar cobranças'
      });
      return;
    }

    this.selectedRegra = regra;
    this.selectedFile = null;
    this.tipoImportacao = 'excel';
    this.jsonInput = '';
    this.displayImportDialog = true;

    // Resetar o componente de upload após o modal abrir
    setTimeout(() => {
      if (this.fileUpload) {
        this.fileUpload.clear();
      }
    });
  }

  baixarModelo(): void {
    if (!this.selectedRegra) return;

    this.regraService.baixarModeloExcel(this.selectedRegra.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `modelo-cobranca-${this.selectedRegra!.nome}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);

        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Modelo Excel baixado com sucesso'
        });
      },
      error: (error) => {
        console.error('Erro ao baixar modelo:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao baixar modelo Excel'
        });
      }
    });
  }

  onFileSelected(event: any): void {
    const file = event.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  fecharImportDialog(): void {
    this.displayImportDialog = false;
    this.selectedFile = null;

    // Resetar o componente de upload
    if (this.fileUpload) {
      this.fileUpload.clear();
    }
  }

  limparArquivo(): void {
    this.selectedFile = null;

    // Resetar o componente de upload para permitir nova seleção
    if (this.fileUpload) {
      this.fileUpload.clear();
    }
  }

  exportarErros(): void {
    if (!this.podeExportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para exportar dados'
      });
      return;
    }

    if (this.errosValidacao.length === 0) {
      return;
    }

    // Criar CSV
    const headers = ['Linha', 'Tipo de Erro', 'Descrição', 'Valor Inválido'];
    const csvContent = [
      headers.join(','),
      ...this.errosValidacao.map(erro =>
        [
          erro.numeroLinha,
          `"${erro.tipoErro}"`,
          `"${erro.descricao.replace(/"/g, '""')}"`,
          erro.valorInvalido ? `"${erro.valorInvalido.replace(/"/g, '""')}"` : '""'
        ].join(',')
      )
    ].join('\n');

    // Criar arquivo e download
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `erros-validacao-${new Date().getTime()}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    this.messageService.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Relatório de erros exportado com sucesso'
    });
  }

  uploadExcel(): void {
    if (!this.selectedRegra || !this.selectedFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Por favor, selecione um arquivo Excel'
      });
      return;
    }

    this.uploadProgress = true;

    this.regraService.importarExcel(this.selectedRegra.id, this.selectedFile).subscribe({
      next: (resultado) => {
        this.uploadProgress = false;
        this.displayImportDialog = false;
        this.selectedFile = null;

        // Resetar o componente de upload
        if (this.fileUpload) {
          this.fileUpload.clear();
        }

        if (resultado.sucesso) {
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: resultado.mensagem
          });
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: resultado.mensagem
          });
        }

        if (resultado.linhasComErro > 0 && resultado.erros && resultado.erros.length > 0) {
          this.errosValidacao = resultado.erros;
          this.displayErrosDialog = true;
          this.messageService.add({
            severity: 'warn',
            summary: 'Atenção',
            detail: `${resultado.linhasComErro} linha(s) com erro. Veja os detalhes no modal.`,
            life: 5000
          });
        }
      },
      error: (err) => {
        console.error('Erro ao importar Excel:', err);
        this.uploadProgress = false;

        // O interceptor preserva o objeto original do backend em err.error
        let mensagemErro = 'Erro ao processar arquivo Excel';
        let detalhesErro = '';

        // Primeiro tenta acessar o objeto preservado pelo interceptor
        const backendError = err.error?.error;

        if (backendError?.mensagem) {
          // Erro do backend (ImportacaoResultado)
          mensagemErro = backendError.mensagem;
          if (backendError.linhasComErro > 0) {
            detalhesErro = `${backendError.linhasComErro} linha(s) com erro`;

            // Armazenar erros de validação se houver
            if (backendError.erros && backendError.erros.length > 0) {
              this.errosValidacao = backendError.erros;
              this.displayErrosDialog = true;
            }
          }
        } else if (err.error?.message) {
          // Mensagem formatada pelo interceptor
          mensagemErro = err.error.message;
        } else if (err.message) {
          // Erro de rede ou HTTP
          mensagemErro = err.message;
        }

        this.messageService.add({
          severity: 'error',
          summary: 'Erro na Importação',
          detail: mensagemErro
        });

        // Se houver detalhes sobre linhas com erro, mostrar também
        if (detalhesErro) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Detalhes',
            detail: detalhesErro
          });
        }
      }
    });
  }

  uploadJson(): void {
    if (!this.selectedRegra || !this.jsonInput.trim()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Por favor, cole um JSON válido'
      });
      return;
    }

    // Validar JSON
    try {
      const cobrancas = JSON.parse(this.jsonInput);

      if (!Array.isArray(cobrancas)) {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'O JSON deve ser um array de cobranças'
        });
        return;
      }

      if (cobrancas.length === 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Atenção',
          detail: 'O JSON não contém nenhuma cobrança'
        });
        return;
      }

      this.uploadProgress = true;

      this.regraService.importarJson(this.selectedRegra.id, cobrancas).subscribe({
        next: (resultado) => {
          this.uploadProgress = false;
          this.displayImportDialog = false;
          this.jsonInput = '';

          if (resultado.sucesso) {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: resultado.mensagem
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: resultado.mensagem
            });
          }

          if (resultado.linhasComErro > 0 && resultado.erros && resultado.erros.length > 0) {
            this.errosValidacao = resultado.erros;
            this.displayErrosDialog = true;
            this.messageService.add({
              severity: 'warn',
              summary: 'Atenção',
              detail: `${resultado.linhasComErro} linha(s) com erro. Veja os detalhes no modal.`,
              life: 5000
            });
          }
        },
        error: (err) => {
          console.error('Erro ao importar JSON:', err);
          this.uploadProgress = false;

          let mensagemErro = 'Erro ao processar JSON';
          const backendError = err.error?.error;

          if (backendError?.mensagem) {
            mensagemErro = backendError.mensagem;
            if (backendError.erros && backendError.erros.length > 0) {
              this.errosValidacao = backendError.erros;
              this.displayErrosDialog = true;
            }
          } else if (err.error?.message) {
            mensagemErro = err.error.message;
          } else if (err.message) {
            mensagemErro = err.message;
          }

          this.messageService.add({
            severity: 'error',
            summary: 'Erro na Importação',
            detail: mensagemErro
          });
        }
      });
    } catch (e) {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'JSON inválido. Verifique a sintaxe e tente novamente.'
      });
    }
  }

  upload(): void {
    if (this.tipoImportacao === 'excel') {
      this.uploadExcel();
    } else {
      this.uploadJson();
    }
  }

  mostrarHistorico(regra: RegraCobranca): void {
    this.selectedRegra = regra;
    this.displayHistoricoDialog = true;
    this.loadingHistorico = true;
    this.limparFiltros();

    this.regraService.getHistoricoImportacoes(regra.id).subscribe({
      next: (historicos) => {
        this.historicoImportacoes = historicos;
        this.historicoFiltrado = historicos;

        // Extrair usuários únicos para o dropdown
        const usuarios = new Set<string>();
        historicos.forEach(h => {
          const usuario = h.nomeUsuario || 'Sistema';
          usuarios.add(usuario);
        });
        this.usuariosUnicos = Array.from(usuarios).sort();

        this.loadingHistorico = false;
      },
      error: (error) => {
        console.error('Erro ao carregar histórico:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar histórico de importações'
        });
        this.loadingHistorico = false;
      }
    });
  }

  verDetalhesHistorico(historico: HistoricoImportacao): void {
    this.selectedHistorico = historico;
    this.displayDetalhesHistoricoDialog = true;
  }

  getStatusImportacaoClass(status: StatusImportacao): string {
    switch (status) {
      case StatusImportacao.Sucesso:
        return 'bg-green-100 text-green-800';
      case StatusImportacao.Parcial:
        return 'bg-yellow-100 text-yellow-800';
      case StatusImportacao.Erro:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  exportarErrosHistorico(historico: HistoricoImportacao): void {
    if (!this.podeExportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para exportar dados'
      });
      return;
    }

    if (!historico.erros || historico.erros.length === 0) {
      return;
    }

    // Criar CSV
    const headers = ['Linha', 'Tipo de Erro', 'Descrição', 'Valor Inválido'];
    const csvContent = [
      headers.join(','),
      ...historico.erros.map(erro =>
        [
          erro.numeroLinha,
          `"${erro.tipoErro}"`,
          `"${erro.descricao.replace(/"/g, '""')}"`,
          erro.valorInvalido ? `"${erro.valorInvalido.replace(/"/g, '""')}"` : '""'
        ].join(',')
      )
    ].join('\n');

    // Criar arquivo e download
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `erros-importacao-${new Date().getTime()}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    this.messageService.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Relatório de erros exportado com sucesso'
    });
  }

  exportarHistoricoExcel(): void {
    if (!this.podeExportar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para exportar dados'
      });
      return;
    }

    if (!this.historicoFiltrado || this.historicoFiltrado.length === 0) {
      return;
    }

    // Criar CSV
    const headers = ['Data/Hora', 'Usuário', 'Arquivo', 'Origem', 'Total Linhas', 'Processadas', 'Com Erro', 'Status'];
    const csvContent = [
      headers.join(','),
      ...this.historicoFiltrado.map(h =>
        [
          `"${new Date(h.dataImportacao).toLocaleString('pt-BR')}"`,
          `"${h.nomeUsuario || 'Sistema'}"`,
          `"${h.nomeArquivo}"`,
          `"${h.origemDescricao}"`,
          h.totalLinhas,
          h.linhasProcessadas,
          h.linhasComErro,
          `"${h.statusDescricao}"`
        ].join(',')
      )
    ].join('\n');

    // Criar arquivo e download
    const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    const nomeRegra = this.historicoFiltrado[0]?.nomeRegra || 'historico';
    link.setAttribute('href', url);
    link.setAttribute('download', `historico-importacoes-${nomeRegra}-${new Date().getTime()}.csv`);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    this.messageService.add({
      severity: 'success',
      summary: 'Sucesso',
      detail: 'Histórico exportado com sucesso'
    });
  }

  aplicarFiltros(): void {
    let resultado = [...this.historicoImportacoes];

    // Filtro por data
    if (this.filtros.dataInicio) {
      const dataInicio = new Date(this.filtros.dataInicio);
      dataInicio.setHours(0, 0, 0, 0);
      resultado = resultado.filter(h => new Date(h.dataImportacao) >= dataInicio);
    }

    if (this.filtros.dataFim) {
      const dataFim = new Date(this.filtros.dataFim);
      dataFim.setHours(23, 59, 59, 999);
      resultado = resultado.filter(h => new Date(h.dataImportacao) <= dataFim);
    }

    // Filtro por origem
    if (this.filtros.origem !== null) {
      resultado = resultado.filter(h => h.origem === this.filtros.origem);
    }

    // Filtro por usuário
    if (this.filtros.usuario) {
      resultado = resultado.filter(h =>
        (h.nomeUsuario || 'Sistema') === this.filtros.usuario
      );
    }

    // Filtro por status
    if (this.filtros.status !== null) {
      resultado = resultado.filter(h => h.status === this.filtros.status);
    }

    this.historicoFiltrado = resultado;
  }

  limparFiltros(): void {
    this.filtros = {
      dataInicio: null,
      dataFim: null,
      origem: null,
      usuario: null,
      status: null
    };
    this.historicoFiltrado = [...this.historicoImportacoes];
  }

  get usuariosOptions() {
    return [
      { label: 'Todos', value: null },
      ...this.usuariosUnicos.map(u => ({ label: u, value: u }))
    ];
  }
}

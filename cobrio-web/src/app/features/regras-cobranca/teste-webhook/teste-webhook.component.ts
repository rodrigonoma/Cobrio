import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { RegraCobrancaService } from '../../../core/services/regra-cobranca.service';
import { RegraCobranca, CanalNotificacao } from '../../../core/models/regra-cobranca.models';
import { MessageService } from 'primeng/api';

interface TestResult {
  status: 'success' | 'error';
  statusCode?: number;
  message: string;
  response?: any;
  timestamp: Date;
}

@Component({
  selector: 'app-teste-webhook',
  templateUrl: './teste-webhook.component.html',
  styleUrls: ['./teste-webhook.component.scss']
})
export class TesteWebhookComponent implements OnInit {
  regras: RegraCobranca[] = [];
  regraSelecionada: RegraCobranca | null = null;
  loading = false;
  sendingTest = false;

  webhookUrl = '';
  payloadJson = '';
  testResults: TestResult[] = [];

  canalNotificacaoLabels = {
    [CanalNotificacao.Email]: 'Email',
    [CanalNotificacao.SMS]: 'SMS',
    [CanalNotificacao.WhatsApp]: 'WhatsApp'
  };

  constructor(
    private regraService: RegraCobrancaService,
    private http: HttpClient,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.loadRegras();
  }

  loadRegras(): void {
    this.loading = true;
    this.regraService.getAll(true).subscribe({
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

  onRegraChange(): void {
    if (!this.regraSelecionada) {
      this.webhookUrl = '';
      this.payloadJson = '';
      return;
    }

    // Gerar URL do webhook
    this.webhookUrl = this.gerarWebhookUrl(this.regraSelecionada.tokenWebhook);

    // Gerar payload de exemplo com as variáveis obrigatórias
    this.gerarPayloadExemplo();
  }

  gerarWebhookUrl(token: string): string {
    const baseUrl = window.location.origin.replace('4201', '5271');
    return `${baseUrl}/api/webhook/${token}`;
  }

  gerarPayloadExemplo(): void {
    if (!this.regraSelecionada) return;

    const payload: any = {};

    // Adicionar destinatário baseado no canal
    switch (this.regraSelecionada.canalNotificacao) {
      case CanalNotificacao.Email:
        payload['Email'] = 'cliente@exemplo.com';
        break;
      case CanalNotificacao.SMS:
      case CanalNotificacao.WhatsApp:
        payload['Telefone'] = '+5511999999999';
        break;
    }

    // Adicionar variáveis obrigatórias do template com valores de exemplo
    this.regraSelecionada.variaveisObrigatorias.forEach(variavel => {
      if (!payload[variavel]) {
        payload[variavel] = this.gerarValorExemplo(variavel);
      }
    });

    // Adicionar data de vencimento baseado na configuração
    if (!payload['DataVencimento']) {
      const dataVencimento = new Date();
      const diasOffset = this.calcularDiasOffset();
      dataVencimento.setDate(dataVencimento.getDate() + diasOffset);
      payload['DataVencimento'] = dataVencimento.toISOString().split('T')[0];
    }

    this.payloadJson = JSON.stringify(payload, null, 2);
  }

  calcularDiasOffset(): number {
    if (!this.regraSelecionada) return 7;

    let dias = this.regraSelecionada.valorTempo;

    // Converter para dias se necessário
    if (this.regraSelecionada.unidadeTempo === 2) { // Horas
      dias = Math.ceil(dias / 24);
    } else if (this.regraSelecionada.unidadeTempo === 1) { // Minutos
      dias = Math.ceil(dias / (24 * 60));
    }

    // Aplicar o tipo de momento
    if (this.regraSelecionada.tipoMomento === 2) { // Depois
      return dias;
    } else if (this.regraSelecionada.tipoMomento === 1) { // Antes
      return dias;
    }

    return dias;
  }

  getDescricaoTempo(): string {
    if (!this.regraSelecionada) return '';

    const momento = this.regraSelecionada.tipoMomento === 1 ? 'Antes' :
                    this.regraSelecionada.tipoMomento === 2 ? 'Depois' : 'Exatamente';
    const unidade = this.regraSelecionada.unidadeTempo === 1 ? 'minutos' :
                    this.regraSelecionada.unidadeTempo === 2 ? 'horas' : 'dias';

    return `${this.regraSelecionada.valorTempo} ${unidade} ${momento.toLowerCase()} do vencimento`;
  }

  gerarValorExemplo(variavel: string): any {
    const lower = variavel.toLowerCase();

    if (lower.includes('nome')) return 'João Silva';
    if (lower.includes('email')) return 'cliente@exemplo.com';
    if (lower.includes('telefone') || lower.includes('celular')) return '+5511999999999';
    if (lower.includes('whatsapp')) return '+5511999999999';
    if (lower.includes('valor')) return '150.00';
    if (lower.includes('data')) return new Date().toISOString().split('T')[0];
    if (lower.includes('link') || lower.includes('url')) return 'https://exemplo.com/pagamento/123';
    if (lower.includes('numero') || lower.includes('codigo')) return '123456';
    if (lower.includes('empresa')) return 'Empresa Exemplo LTDA';
    if (lower.includes('documento') || lower.includes('cpf')) return '123.456.789-00';
    if (lower.includes('cnpj')) return '12.345.678/0001-90';

    return 'valor_exemplo';
  }

  formatarJson(): void {
    try {
      const parsed = JSON.parse(this.payloadJson);
      this.payloadJson = JSON.stringify(parsed, null, 2);
      this.messageService.add({
        severity: 'success',
        summary: 'JSON Formatado',
        detail: 'JSON formatado com sucesso'
      });
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'JSON Inválido',
        detail: 'O JSON digitado está inválido'
      });
    }
  }

  validarPayload(): boolean {
    if (!this.regraSelecionada) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Selecione uma regra de cobrança'
      });
      return false;
    }

    try {
      const payload = JSON.parse(this.payloadJson);

      // Validar se todas as variáveis obrigatórias estão presentes
      const variaveisFaltando = this.regraSelecionada.variaveisObrigatorias.filter(
        variavel => !payload.hasOwnProperty(variavel)
      );

      if (variaveisFaltando.length > 0) {
        this.messageService.add({
          severity: 'warn',
          summary: 'Variáveis Faltando',
          detail: `As seguintes variáveis são obrigatórias: ${variaveisFaltando.join(', ')}`
        });
        return false;
      }

      return true;
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'JSON Inválido',
        detail: 'O JSON digitado está inválido'
      });
      return false;
    }
  }

  enviarTeste(): void {
    if (!this.validarPayload()) {
      return;
    }

    this.sendingTest = true;

    const payload = JSON.parse(this.payloadJson);

    this.http.post(this.webhookUrl, payload).subscribe({
      next: (response: any) => {
        this.testResults.unshift({
          status: 'success',
          statusCode: 200,
          message: 'Cobrança recebida e processada com sucesso',
          response: response,
          timestamp: new Date()
        });

        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Teste enviado com sucesso! A cobrança será processada em breve.'
        });

        this.sendingTest = false;
      },
      error: (error: HttpErrorResponse) => {
        this.testResults.unshift({
          status: 'error',
          statusCode: error.status,
          message: error.error?.message || error.message || 'Erro ao enviar teste',
          response: error.error,
          timestamp: new Date()
        });

        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.error?.message || 'Erro ao enviar teste de webhook'
        });

        this.sendingTest = false;
      }
    });
  }

  copiarWebhookUrl(): void {
    navigator.clipboard.writeText(this.webhookUrl).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado!',
        detail: 'URL do webhook copiada para área de transferência'
      });
    });
  }

  copiarPayload(): void {
    navigator.clipboard.writeText(this.payloadJson).then(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Copiado!',
        detail: 'Payload copiado para área de transferência'
      });
    });
  }

  limparHistorico(): void {
    this.testResults = [];
    this.messageService.add({
      severity: 'info',
      summary: 'Histórico Limpo',
      detail: 'Histórico de testes foi limpo'
    });
  }

  getCanalLabel(canal: CanalNotificacao): string {
    return this.canalNotificacaoLabels[canal] || 'Desconhecido';
  }
}

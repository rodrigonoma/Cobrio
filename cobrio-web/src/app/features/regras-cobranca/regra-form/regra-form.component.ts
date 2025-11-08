import { Component, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RegraCobrancaService } from '../../../core/services/regra-cobranca.service';
import { MessageService } from 'primeng/api';
import { TipoMomento, UnidadeTempo } from '../../../core/models/regra-cobranca.models';
import { Editor } from 'primeng/editor';

@Component({
  selector: 'app-regra-form',
  templateUrl: './regra-form.component.html',
  styleUrls: ['./regra-form.component.scss']
})
export class RegraFormComponent implements OnInit {
  @ViewChild('editor') editor!: Editor;

  form!: FormGroup;
  loading = false;
  editMode = false;
  regraId: string | null = null;
  ehPadrao = false;

  canaisNotificacao = [
    { label: 'Email', value: 1 },
    { label: 'SMS', value: 2 },
    { label: 'WhatsApp', value: 3 }
  ];

  tiposMomento = [
    { label: 'Antes', value: TipoMomento.Antes },
    { label: 'Depois', value: TipoMomento.Depois },
    { label: 'Exatamente', value: TipoMomento.Exatamente }
  ];

  unidadesTempo = [
    { label: 'Minutos', value: UnidadeTempo.Minutos },
    { label: 'Horas', value: UnidadeTempo.Horas },
    { label: 'Dias', value: UnidadeTempo.Dias }
  ];

  templateExemplo = `<p><strong>Olá,</strong></p>
<p>Sua fatura no valor de <span style="color: rgb(230, 0, 0);">R$ {{Valor}}</span> vence em <strong>{{DataVencimento}}</strong>.</p>
<p>Para pagar, acesse: <a href="{{LinkPagamento}}" target="_blank">Clique aqui para pagar</a></p>
<p><br></p>
<p>Atenciosamente,</p>
<p><em>Equipe {{NomeEmpresa}}</em></p>`;

  variaveisExtraidas: string[] = [];
  variaveisObrigatoriasSistema: string[] = [];
  novaVariavelSistema: string = '';
  webhookUrl: string = '';
  editorInstance: any;

  constructor(
    private fb: FormBuilder,
    private regraService: RegraCobrancaService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.checkEditMode();
  }

  initForm(): void {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.maxLength(200)]],
      descricao: ['', Validators.maxLength(1000)],
      tipoMomento: [TipoMomento.Antes, Validators.required],
      valorTempo: [3, [Validators.required, Validators.min(1), Validators.max(9999)]],
      unidadeTempo: [UnidadeTempo.Dias, Validators.required],
      canalNotificacao: [1, Validators.required],
      templateNotificacao: ['', Validators.required],
      subjectEmail: ['', [Validators.required, Validators.maxLength(150)]]
    });

    // Extrair variáveis ao mudar o template
    this.form.get('templateNotificacao')?.valueChanges.subscribe(value => {
      this.extrairVariaveis(value);
    });

    // Atualizar variáveis obrigatórias e validação de subject ao mudar o canal de notificação
    this.form.get('canalNotificacao')?.valueChanges.subscribe(canal => {
      this.atualizarVariavelObrigatoriaSistema(canal);
      this.atualizarValidacaoSubject(canal);
    });

    // Inicializar variável obrigatória com base no canal padrão (Email)
    this.atualizarVariavelObrigatoriaSistema(1);
    this.atualizarValidacaoSubject(1);
  }

  checkEditMode(): void {
    this.regraId = this.route.snapshot.paramMap.get('id');
    if (this.regraId) {
      this.editMode = true;
      this.loadRegra();
    }
  }

  loadRegra(): void {
    if (!this.regraId) return;

    this.loading = true;
    this.regraService.getById(this.regraId).subscribe({
      next: (regra) => {
        this.ehPadrao = regra.ehPadrao;

        // Se for regra padrão (Envio Imediato), limpar campos de momento/tempo
        if (this.ehPadrao) {
          this.form.patchValue({
            nome: regra.nome,
            descricao: regra.descricao,
            tipoMomento: null,
            valorTempo: 0,
            unidadeTempo: null,
            canalNotificacao: regra.canalNotificacao,
            templateNotificacao: regra.templateNotificacao,
            subjectEmail: regra.subjectEmail || ''
          });

          // Desabilitar campos que não podem ser editados
          this.form.get('nome')?.disable();
          this.form.get('descricao')?.disable();
          this.form.get('tipoMomento')?.disable();
          this.form.get('valorTempo')?.disable();
          this.form.get('unidadeTempo')?.disable();
          this.form.get('templateNotificacao')?.disable();

          // Remover validadores obrigatórios dos campos de tempo para régua padrão
          this.form.get('tipoMomento')?.clearValidators();
          this.form.get('valorTempo')?.clearValidators();
          this.form.get('unidadeTempo')?.clearValidators();
          this.form.get('tipoMomento')?.updateValueAndValidity();
          this.form.get('valorTempo')?.updateValueAndValidity();
          this.form.get('unidadeTempo')?.updateValueAndValidity();
        } else {
          // Régua normal - carregar valores normalmente
          this.form.patchValue({
            nome: regra.nome,
            descricao: regra.descricao,
            tipoMomento: regra.tipoMomento,
            valorTempo: regra.valorTempo,
            unidadeTempo: regra.unidadeTempo,
            canalNotificacao: regra.canalNotificacao,
            templateNotificacao: regra.templateNotificacao,
            subjectEmail: regra.subjectEmail || ''
          });
        }

        // Garantir que o editor Quill seja atualizado após inicialização
        setTimeout(() => {
          if (this.editor) {
            const quill = this.editor.getQuill();
            if (quill && regra.templateNotificacao) {
              quill.root.innerHTML = regra.templateNotificacao;
            }
          }
          this.extrairVariaveis(regra.templateNotificacao);
        }, 100);

        this.webhookUrl = this.gerarWebhookUrl(regra.tokenWebhook);

        // Carregar variáveis obrigatórias do sistema
        if (regra.variaveisObrigatoriasSistema) {
          try {
            this.variaveisObrigatoriasSistema = JSON.parse(regra.variaveisObrigatoriasSistema);
          } catch (e) {
            this.variaveisObrigatoriasSistema = [];
          }
        }

        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar regra:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar regra de cobrança'
        });
        this.loading = false;
        this.voltar();
      }
    });
  }

  extrairVariaveis(template: string): void {
    if (!template) {
      this.variaveisExtraidas = [];
      return;
    }

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

    this.variaveisExtraidas = Array.from(variaveis).sort();
  }

  inserirVariavel(variavel: string): void {
    const control = this.form.get('templateNotificacao');
    if (!control) return;

    if (!this.editor) return;

    const quill = this.editor.getQuill();
    if (!quill) return;

    // Dar foco ao editor
    quill.focus();

    // Pegar a posição atual do cursor ou final do documento
    const selection = quill.getSelection(true);
    const cursorPosition = selection ? selection.index : quill.getLength() - 1;

    // Inserir a variável na posição do cursor
    const variavelTexto = ` {{${variavel}}} `;
    quill.insertText(cursorPosition, variavelTexto);

    // Formatar a variável inserida com destaque visual
    quill.formatText(
      cursorPosition + 1,
      variavelTexto.length - 2,
      {
        'background': '#e3f2fd',
        'color': '#1565c0',
        'code': true
      }
    );

    // Mover cursor para depois da variável
    quill.setSelection(cursorPosition + variavelTexto.length, 0);

    // Extrair variáveis do novo HTML
    setTimeout(() => {
      const novoHtml = quill.root.innerHTML;
      this.extrairVariaveis(novoHtml);
    }, 0);
  }

  inserirVariavelNoSubject(variavel: string): void {
    const control = this.form.get('subjectEmail');
    if (!control) return;

    const currentValue = control.value || '';
    const newValue = currentValue + ` {{${variavel}}}`;

    // Verificar se não ultrapassa o limite de 150 caracteres
    if (newValue.length > 150) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'O assunto não pode ultrapassar 150 caracteres'
      });
      return;
    }

    control.setValue(newValue);
    control.markAsTouched();
  }

  gerarWebhookUrl(token: string): string {
    const baseUrl = window.location.origin.replace('4201', '5271');
    return `${baseUrl}/api/webhook/${token}`;
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

  adicionarVariavelSistema(): void {
    const variavel = this.novaVariavelSistema.trim();

    if (!variavel) {
      return;
    }

    // Verificar se já existe
    if (this.variaveisObrigatoriasSistema.includes(variavel)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Essa variável já foi adicionada'
      });
      return;
    }

    this.variaveisObrigatoriasSistema.push(variavel);
    this.novaVariavelSistema = '';
  }

  removerVariavelSistema(variavel: string): void {
    const index = this.variaveisObrigatoriasSistema.indexOf(variavel);
    if (index > -1) {
      this.variaveisObrigatoriasSistema.splice(index, 1);
    }
  }

  atualizarVariavelObrigatoriaSistema(canal: number): void {
    // Remove variáveis de sistema anteriores (Email ou Telefone)
    this.variaveisObrigatoriasSistema = this.variaveisObrigatoriasSistema.filter(
      v => v !== 'Email' && v !== 'Telefone'
    );

    // Adiciona a variável apropriada baseada no canal
    if (canal === 1) { // Email
      if (!this.variaveisObrigatoriasSistema.includes('Email')) {
        this.variaveisObrigatoriasSistema.unshift('Email');
      }
    } else if (canal === 2 || canal === 3) { // SMS ou WhatsApp
      if (!this.variaveisObrigatoriasSistema.includes('Telefone')) {
        this.variaveisObrigatoriasSistema.unshift('Telefone');
      }
    }
  }

  atualizarValidacaoSubject(canal: number): void {
    const subjectControl = this.form.get('subjectEmail');

    if (canal === 1) { // Email
      subjectControl?.setValidators([Validators.required, Validators.maxLength(150)]);
    } else {
      subjectControl?.clearValidators();
    }

    subjectControl?.updateValueAndValidity();
  }

  salvar(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Por favor, preencha todos os campos obrigatórios corretamente'
      });
      return;
    }

    this.loading = true;
    const formData = {
      ...this.form.value,
      variaveisObrigatoriasSistema: this.variaveisObrigatoriasSistema.length > 0
        ? this.variaveisObrigatoriasSistema
        : null
    };

    const observable = this.editMode && this.regraId
      ? this.regraService.update(this.regraId, formData)
      : this.regraService.create(formData);

    observable.subscribe({
      next: (response: any) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `Regra ${this.editMode ? 'atualizada' : 'criada'} com sucesso`
        });

        // Se criou uma nova regra, mostrar o webhook
        if (!this.editMode && response.tokenWebhook) {
          this.webhookUrl = this.gerarWebhookUrl(response.tokenWebhook);
          this.messageService.add({
            severity: 'info',
            summary: 'Webhook Gerado',
            detail: 'URL do webhook disponível abaixo',
            life: 5000
          });
        }

        this.loading = false;

        // Aguardar um pouco antes de voltar para dar tempo do toast aparecer
        if (this.editMode) {
          setTimeout(() => {
            this.voltar();
          }, 1500);
        }
      },
      error: (error) => {
        console.error('Erro ao salvar regra:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: `Erro ao ${this.editMode ? 'atualizar' : 'criar'} regra`
        });
        this.loading = false;
      }
    });
  }

  voltar(): void {
    this.router.navigate(['/regras-cobranca']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  getFieldError(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.touched || !control.errors) return '';

    const errors = control.errors;
    if (errors['required']) return 'Campo obrigatório';
    if (errors['maxLength']) return `Máximo de ${errors['maxLength'].requiredLength} caracteres`;
    if (errors['min']) return `Valor mínimo: ${errors['min'].min}`;
    if (errors['max']) return `Valor máximo: ${errors['max'].max}`;
    if (errors['pattern']) return 'URL inválida (deve começar com http:// ou https://)';
    if (errors['invalidJson']) return 'JSON inválido';

    return 'Campo inválido';
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!(control && control.invalid && control.touched);
  }
}

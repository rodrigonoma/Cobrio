import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfiguracoesService } from '../../../core/services/configuracoes.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-email-config',
  templateUrl: './email-config.component.html',
  styleUrls: ['./email-config.component.scss']
})
export class EmailConfigComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  salvando = false;

  constructor(
    private fb: FormBuilder,
    private configService: ConfiguracoesService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadConfig();
  }

  initForm(): void {
    this.form = this.fb.group({
      emailRemetente: ['', [Validators.email, Validators.maxLength(200)]],
      nomeRemetente: ['', Validators.maxLength(200)],
      emailReplyTo: ['', [Validators.email, Validators.maxLength(200)]]
    });
  }

  loadConfig(): void {
    this.loading = true;
    this.configService.getEmailConfig().subscribe({
      next: (config) => {
        this.form.patchValue({
          emailRemetente: config.emailRemetente || '',
          nomeRemetente: config.nomeRemetente || '',
          emailReplyTo: config.emailReplyTo || ''
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar configurações:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar configurações de email'
        });
        this.loading = false;
      }
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Por favor, corrija os erros no formulário'
      });
      return;
    }

    this.salvando = true;
    const config = this.form.value;

    this.configService.updateEmailConfig(config).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Configurações de email atualizadas com sucesso'
        });
        this.salvando = false;
      },
      error: (error) => {
        console.error('Erro ao salvar configurações:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.error?.message || 'Erro ao salvar configurações'
        });
        this.salvando = false;
      }
    });
  }

  limparCampo(campo: string): void {
    this.form.get(campo)?.setValue('');
    this.form.get(campo)?.markAsTouched();
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
    if (errors['email']) return 'Email inválido';
    if (errors['maxLength']) return `Máximo de ${errors['maxLength'].requiredLength} caracteres`;

    return 'Campo inválido';
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!(control && control.invalid && control.touched);
  }
}

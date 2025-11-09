import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TemplateEmailService } from '../../../core/services/template-email.service';
import { TemplateEmail } from '../../../core/models/template-email.models';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-template-form',
  templateUrl: './template-form.component.html',
  styleUrls: ['./template-form.component.scss'],
  providers: [MessageService]
})
export class TemplateFormComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  editMode = false;
  templateId: string | null = null;
  template: TemplateEmail | null = null;

  canaisNotificacao = [
    { label: 'Email', value: 1 },
    { label: 'SMS', value: 2 },
    { label: 'WhatsApp', value: 3 }
  ];

  constructor(
    private fb: FormBuilder,
    private templateService: TemplateEmailService,
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
      conteudoHtml: ['', Validators.required],
      subjectEmail: ['', Validators.maxLength(150)],
      canalSugerido: [null]
    });
  }

  checkEditMode(): void {
    this.templateId = this.route.snapshot.paramMap.get('id');
    if (this.templateId) {
      this.editMode = true;
      this.loadTemplate();
    }
  }

  loadTemplate(): void {
    if (!this.templateId) return;

    this.loading = true;
    this.templateService.getById(this.templateId).subscribe({
      next: (template) => {
        this.template = template;
        this.form.patchValue({
          nome: template.nome,
          descricao: template.descricao,
          conteudoHtml: template.conteudoHtml,
          subjectEmail: template.subjectEmail,
          canalSugerido: template.canalSugerido
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar template:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar template'
        });
        this.loading = false;
        this.voltar();
      }
    });
  }

  salvar(): void {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Por favor, preencha todos os campos obrigatórios'
      });
      return;
    }

    this.loading = true;
    const formData = this.form.value;

    const observable = this.editMode && this.templateId
      ? this.templateService.update(this.templateId, formData)
      : this.templateService.create(formData);

    observable.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `Template ${this.editMode ? 'atualizado' : 'criado'} com sucesso`
        });
        this.loading = false;
        setTimeout(() => this.voltar(), 1500);
      },
      error: (error) => {
        console.error('Erro ao salvar template:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.error?.message || 'Erro ao salvar template'
        });
        this.loading = false;
      }
    });
  }

  voltar(): void {
    this.router.navigate(['/templates']);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!(control && control.invalid && control.touched);
  }

  getFieldError(fieldName: string): string {
    const control = this.form.get(fieldName);
    if (!control || !control.touched || !control.errors) return '';

    const errors = control.errors;
    if (errors['required']) return 'Campo obrigatório';
    if (errors['maxlength']) return `Máximo de ${errors['maxlength'].requiredLength} caracteres`;

    return 'Campo inválido';
  }
}

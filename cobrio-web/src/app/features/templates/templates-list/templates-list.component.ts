import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TemplateEmailService } from '../../../core/services/template-email.service';
import { TemplateEmail } from '../../../core/models/template-email.models';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-templates-list',
  templateUrl: './templates-list.component.html',
  styleUrls: ['./templates-list.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class TemplatesListComponent implements OnInit {
  templates: TemplateEmail[] = [];
  loading = false;
  selectedTemplate: TemplateEmail | null = null;
  showPreviewDialog = false;

  canaisNotificacao = [
    { label: 'Email', value: 1 },
    { label: 'SMS', value: 2 },
    { label: 'WhatsApp', value: 3 }
  ];

  constructor(
    private templateService: TemplateEmailService,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.loadTemplates();
  }

  loadTemplates(): void {
    this.loading = true;
    this.templateService.getAll().subscribe({
      next: (templates) => {
        this.templates = templates;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar templates:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar templates'
        });
        this.loading = false;
      }
    });
  }

  novoTemplate(): void {
    this.router.navigate(['/templates/novo']);
  }

  editarTemplate(id: string): void {
    this.router.navigate(['/templates/editar', id]);
  }

  confirmarExclusao(template: TemplateEmail): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o template "${template.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim, excluir',
      rejectLabel: 'Cancelar',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.excluirTemplate(template.id);
      }
    });
  }

  excluirTemplate(id: string): void {
    this.loading = true;
    this.templateService.delete(id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Template excluído com sucesso'
        });
        this.loadTemplates();
      },
      error: (error) => {
        console.error('Erro ao excluir template:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao excluir template'
        });
        this.loading = false;
      }
    });
  }

  visualizarTemplate(template: TemplateEmail): void {
    this.selectedTemplate = template;
    this.showPreviewDialog = true;
  }

  closePreviewDialog(): void {
    this.showPreviewDialog = false;
    this.selectedTemplate = null;
  }

  getCanalLabel(canal: number | null | undefined): string {
    if (!canal) return 'Não definido';
    const canalInfo = this.canaisNotificacao.find(c => c.value === canal);
    return canalInfo ? canalInfo.label : 'Não definido';
  }

  getCanalIcon(canal: number | null | undefined): string {
    if (!canal) return 'pi pi-question';
    switch (canal) {
      case 1: return 'pi pi-envelope';
      case 2: return 'pi pi-mobile';
      case 3: return 'pi pi-whatsapp';
      default: return 'pi pi-question';
    }
  }

  getCanalSeverity(canal: number | null | undefined): string {
    if (!canal) return 'secondary';
    switch (canal) {
      case 1: return 'info';
      case 2: return 'warning';
      case 3: return 'success';
      default: return 'secondary';
    }
  }
}

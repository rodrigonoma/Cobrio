import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AssinaturaService } from '../../../core/services/assinatura.service';
import { PlanoOfertaService } from '../../../core/services/plano-oferta.service';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';
import {
  Assinatura,
  CreateAssinaturaRequest,
  CancelarAssinaturaRequest,
  STATUS_ASSINATURA_OPTIONS,
  StatusAssinatura
} from '../../../core/models/assinatura.models';
import { PlanoOferta } from '../../../core/models/plano-oferta.models';

@Component({
  selector: 'app-assinaturas-list',
  templateUrl: './assinaturas-list.component.html',
  styleUrls: ['./assinaturas-list.component.scss']
})
export class AssinaturasListComponent implements OnInit {
  assinaturas: Assinatura[] = [];
  planos: PlanoOferta[] = [];
  loading = false;
  displayFormDialog = false;
  displayDetailsDialog = false;
  displayCancelDialog = false;
  assinaturaForm!: FormGroup;
  cancelForm!: FormGroup;
  selectedAssinatura: Assinatura | null = null;
  editMode = false;
  statusOptions = STATUS_ASSINATURA_OPTIONS;

  // Permissões dinâmicas
  perfilUsuarioString: string = 'Admin';
  podeVisualizar = false;
  podeCriar = false;
  podeEditar = false;
  podeExcluir = false;

  constructor(
    private assinaturaService: AssinaturaService,
    private planoService: PlanoOfertaService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private authService: AuthService,
    private permissaoService: PermissaoService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Carregar perfil do usuário e suas permissões
    this.authService.currentUser$.subscribe(user => {
      if (user?.perfil) {
        this.perfilUsuarioString = user.perfil;
        this.carregarPermissoes();
      }
    });

    this.initForms();
    this.loadAssinaturas();
    this.loadPlanos();
  }

  carregarPermissoes(): void {
    const moduloChave = 'assinaturas';

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
  }

  // Métodos helper para verificar permissões
  canCreate(): boolean {
    return this.podeCriar;
  }

  canEdit(): boolean {
    return this.podeEditar;
  }

  canDelete(): boolean {
    return this.podeExcluir;
  }

  initForms(): void {
    this.assinaturaForm = this.fb.group({
      planoOfertaId: ['', Validators.required],
      nome: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      telefone: [''],
      cpfCnpj: [''],
      logradouro: [''],
      numero: [''],
      complemento: [''],
      bairro: [''],
      cidade: [''],
      estado: [''],
      cep: [''],
      pais: ['Brasil'],
      numeroCartao: [''],
      nomeTitular: [''],
      validadeCartao: [''],
      cvv: [''],
      iniciarEmTrial: [true],
      dataInicio: [new Date()]
    });

    this.cancelForm = this.fb.group({
      motivo: [''],
      cancelarImediatamente: [false]
    });
  }

  loadAssinaturas(): void {
    this.loading = true;
    this.assinaturaService.getAll().subscribe({
      next: (assinaturas) => {
        this.assinaturas = assinaturas;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar assinaturas', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar assinaturas'
        });
        this.loading = false;
      }
    });
  }

  loadPlanos(): void {
    this.planoService.getAll().subscribe({
      next: (planos) => {
        this.planos = planos.filter(p => p.ativo);
      },
      error: (error) => {
        console.error('Erro ao carregar planos', error);
      }
    });
  }

  openNewAssinaturaDialog(): void {
    this.editMode = false;
    this.assinaturaForm.reset({
      pais: 'Brasil',
      iniciarEmTrial: true,
      dataInicio: new Date()
    });
    this.displayFormDialog = true;
  }

  saveAssinatura(): void {
    if (this.assinaturaForm.invalid) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Atenção',
        detail: 'Preencha todos os campos obrigatórios'
      });
      return;
    }

    const formValue = this.assinaturaForm.value;
    const requestData: CreateAssinaturaRequest = {
      ...formValue,
      dataInicio: formValue.dataInicio ? new Date(formValue.dataInicio).toISOString() : undefined
    };

    this.assinaturaService.create(requestData).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Assinatura criada com sucesso'
        });
        this.displayFormDialog = false;
        this.loadAssinaturas();
      },
      error: (error) => {
        console.error('Erro ao criar assinatura', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.error?.message || 'Erro ao criar assinatura'
        });
      }
    });
  }

  showDetails(assinatura: Assinatura): void {
    this.selectedAssinatura = assinatura;
    this.displayDetailsDialog = true;
  }

  openCancelDialog(assinatura: Assinatura): void {
    this.selectedAssinatura = assinatura;
    this.cancelForm.reset({ cancelarImediatamente: false });
    this.displayCancelDialog = true;
  }

  cancelarAssinatura(): void {
    if (!this.selectedAssinatura) return;

    const request: CancelarAssinaturaRequest = {
      motivo: this.cancelForm.value.motivo,
      cancelarImediatamente: this.cancelForm.value.cancelarImediatamente
    };

    this.assinaturaService.cancelar(this.selectedAssinatura.id, request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Assinatura cancelada com sucesso'
        });
        this.displayCancelDialog = false;
        this.loadAssinaturas();
      },
      error: (error) => {
        console.error('Erro ao cancelar assinatura', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao cancelar assinatura'
        });
      }
    });
  }

  suspenderAssinatura(assinatura: Assinatura): void {
    this.confirmationService.confirm({
      message: `Deseja suspender a assinatura de ${assinatura.assinanteNome}?`,
      header: 'Confirmar Suspensão',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.assinaturaService.suspender(assinatura.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Assinatura suspensa com sucesso'
            });
            this.loadAssinaturas();
          },
          error: (error) => {
            console.error('Erro ao suspender assinatura', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao suspender assinatura'
            });
          }
        });
      }
    });
  }

  reativarAssinatura(assinatura: Assinatura): void {
    this.confirmationService.confirm({
      message: `Deseja reativar a assinatura de ${assinatura.assinanteNome}?`,
      header: 'Confirmar Reativação',
      icon: 'pi pi-question-circle',
      accept: () => {
        this.assinaturaService.reativar(assinatura.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Assinatura reativada com sucesso'
            });
            this.loadAssinaturas();
          },
          error: (error) => {
            console.error('Erro ao reativar assinatura', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao reativar assinatura'
            });
          }
        });
      }
    });
  }

  confirmDelete(assinatura: Assinatura): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir a assinatura de ${assinatura.assinanteNome}? Esta ação não pode ser desfeita.`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.assinaturaService.delete(assinatura.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Assinatura excluída com sucesso'
            });
            this.loadAssinaturas();
          },
          error: (error) => {
            console.error('Erro ao excluir assinatura', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: 'Erro ao excluir assinatura'
            });
          }
        });
      }
    });
  }

  getStatusSeverity(status: StatusAssinatura): string {
    const option = this.statusOptions.find(opt => opt.value === status);
    return option?.severity || 'info';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value / 100);
  }

  formatDate(date: string | undefined): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('pt-BR');
  }

  canCancel(assinatura: Assinatura): boolean {
    return assinatura.status === 'Ativa' || assinatura.status === 'EmTrial';
  }

  canSuspend(assinatura: Assinatura): boolean {
    return assinatura.status === 'Ativa' || assinatura.status === 'EmTrial';
  }

  canReactivate(assinatura: Assinatura): boolean {
    return assinatura.status === 'Suspensa';
  }
}

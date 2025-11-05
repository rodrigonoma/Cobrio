import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { PlanoOfertaService } from '../../../core/services/plano-oferta.service';
import { PlanoOferta, TIPOS_CICLO, TipoCiclo } from '../../../core/models/plano-oferta.models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';

@Component({
  selector: 'app-planos-list',
  templateUrl: './planos-list.component.html',
  styleUrls: ['./planos-list.component.scss']
})
export class PlanosListComponent implements OnInit {
  planos: PlanoOferta[] = [];
  loading = false;
  displayDialog = false;
  displayFormDialog = false;
  selectedPlano: PlanoOferta | null = null;
  planoForm!: FormGroup;
  editMode = false;
  submitting = false;
  tiposCiclo = TIPOS_CICLO;

  // Permissões dinâmicas
  perfilUsuarioString: string = 'Admin';
  podeVisualizar = false;
  podeCriar = false;
  podeEditar = false;
  podeExcluir = false;
  podeAlterarStatus = false;

  constructor(
    private planoService: PlanoOfertaService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private permissaoService: PermissaoService
  ) {
    this.initForm();
  }

  ngOnInit(): void {
    // Carregar perfil do usuário e suas permissões
    this.authService.currentUser$.subscribe(user => {
      if (user?.perfil) {
        this.perfilUsuarioString = user.perfil;
        this.carregarPermissoes();
      }
    });

    this.loadPlanos();
  }

  carregarPermissoes(): void {
    const moduloChave = 'planos';

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

    // Verificar permissão de ativar/desativar (toggle)
    this.permissaoService.verificarPermissao(this.perfilUsuarioString, moduloChave, 'toggle').subscribe({
      next: (result) => this.podeAlterarStatus = result.permitido,
      error: (err) => console.error('Erro ao verificar permissão de alteração de status:', err)
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

  canToggleStatus(): boolean {
    return this.podeAlterarStatus;
  }

  initForm(): void {
    this.planoForm = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(3)]],
      descricao: [''],
      valor: [0, [Validators.required, Validators.min(0)]],
      tipoCiclo: ['Mensal', Validators.required],
      periodoTrial: [0, [Validators.required, Validators.min(0)]],
      limiteUsuarios: [null],
      permiteUpgrade: [true],
      permiteDowngrade: [true]
    });
  }

  loadPlanos(): void {
    this.loading = true;
    this.planoService.getAll().subscribe({
      next: (data) => {
        this.planos = data;
        this.loading = false;
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.message || 'Erro ao carregar planos'
        });
      }
    });
  }

  openNewPlanoDialog(): void {
    this.editMode = false;
    this.selectedPlano = null;
    this.planoForm.reset({
      nome: '',
      descricao: '',
      valor: 0,
      tipoCiclo: 'Mensal',
      periodoTrial: 0,
      limiteUsuarios: null,
      permiteUpgrade: true,
      permiteDowngrade: true
    });
    this.displayFormDialog = true;
  }

  openEditPlanoDialog(plano: PlanoOferta): void {
    this.editMode = true;
    this.selectedPlano = plano;
    this.planoForm.patchValue({
      nome: plano.nome,
      descricao: plano.descricao || '',
      valor: plano.valor / 100, // Converte de centavos para reais
      tipoCiclo: plano.tipoCiclo,
      periodoTrial: plano.periodoTrial,
      limiteUsuarios: plano.limiteUsuarios,
      permiteUpgrade: plano.permiteUpgrade,
      permiteDowngrade: plano.permiteDowngrade
    });
    this.displayFormDialog = true;
  }

  savePlano(): void {
    if (this.planoForm.invalid) {
      Object.keys(this.planoForm.controls).forEach(key => {
        this.planoForm.controls[key].markAsTouched();
      });
      return;
    }

    this.submitting = true;
    const formValue = this.planoForm.value;

    // Converte valor de reais para centavos
    const requestData = {
      ...formValue,
      valor: Math.round(formValue.valor * 100)
    };

    if (this.editMode && this.selectedPlano) {
      this.planoService.update(this.selectedPlano.id, requestData).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Plano atualizado com sucesso'
          });
          this.displayFormDialog = false;
          this.submitting = false;
          this.loadPlanos();
        },
        error: (error) => {
          this.submitting = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: error.message || 'Erro ao atualizar plano'
          });
        }
      });
    } else {
      this.planoService.create(requestData).subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Sucesso',
            detail: 'Plano criado com sucesso'
          });
          this.displayFormDialog = false;
          this.submitting = false;
          this.loadPlanos();
        },
        error: (error) => {
          this.submitting = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Erro',
            detail: error.message || 'Erro ao criar plano'
          });
        }
      });
    }
  }

  showDetails(plano: PlanoOferta): void {
    this.selectedPlano = plano;
    this.displayDialog = true;
  }

  confirmDelete(plano: PlanoOferta): void {
    this.confirmationService.confirm({
      message: `Tem certeza que deseja excluir o plano "${plano.nome}"?`,
      header: 'Confirmar Exclusão',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.deletePlano(plano.id);
      }
    });
  }

  deletePlano(id: string): void {
    this.planoService.delete(id).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Plano excluído com sucesso'
        });
        this.loadPlanos();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.message || 'Erro ao excluir plano'
        });
      }
    });
  }

  toggleStatus(plano: PlanoOferta): void {
    const action = plano.ativo ? 'desativar' : 'ativar';
    const service = plano.ativo
      ? this.planoService.deactivate(plano.id)
      : this.planoService.activate(plano.id);

    service.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: `Plano ${action}do com sucesso`
        });
        this.loadPlanos();
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.message || `Erro ao ${action} plano`
        });
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(value / 100);
  }

  getSeverity(ativo: boolean): string {
    return ativo ? 'success' : 'danger';
  }

  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  hasError(field: string): boolean {
    const control = this.planoForm.get(field);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  getErrorMessage(field: string): string {
    const control = this.planoForm.get(field);
    if (control?.hasError('required')) {
      return 'Este campo é obrigatório';
    }
    if (control?.hasError('minlength')) {
      return `Mínimo de ${control.errors?.['minlength'].requiredLength} caracteres`;
    }
    if (control?.hasError('min')) {
      return 'Valor deve ser maior ou igual a zero';
    }
    return '';
  }
}

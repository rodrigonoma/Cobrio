import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UsuarioEmpresaService } from '../../../core/services/usuario-empresa.service';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';
import { UsuarioEmpresa, PerfilUsuario } from '../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-usuarios-list',
  templateUrl: './usuarios-list.component.html',
  styleUrls: ['./usuarios-list.component.scss']
})
export class UsuariosListComponent implements OnInit {
  usuarios: UsuarioEmpresa[] = [];
  loading = true;
  displayResetarSenhaDialog = false;
  selectedUsuario: UsuarioEmpresa | null = null;
  novaSenha: string = '';
  confirmSenha: string = '';
  PerfilUsuario = PerfilUsuario;

  // Permissões
  perfilUsuarioString: string = '';
  podeVisualizar = false;
  podeCriar = false;
  podeEditar = false;
  podeExcluir = false;
  podeResetarSenha = false;

  constructor(
    private usuarioService: UsuarioEmpresaService,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private authService: AuthService,
    private permissaoService: PermissaoService
  ) { }

  ngOnInit(): void {
    this.carregarPermissoes();
    this.loadUsuarios();
  }

  loadUsuarios(): void {
    this.loading = true;
    this.usuarioService.getAll().subscribe({
      next: (usuarios) => {
        this.usuarios = usuarios;
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar usuários:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar usuários'
        });
        this.loading = false;
      }
    });
  }

  novoUsuario(): void {
    if (!this.podeCriar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para criar usuários'
      });
      return;
    }
    this.router.navigate(['/usuarios/novo']);
  }

  editarUsuario(usuario: UsuarioEmpresa): void {
    if (!this.podeEditar) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para editar usuários'
      });
      return;
    }

    if (usuario.ehProprietario) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Ação não permitida',
        detail: 'O usuário proprietário não pode ser editado'
      });
      return;
    }
    this.router.navigate(['/usuarios/editar', usuario.id]);
  }

  desativarUsuario(usuario: UsuarioEmpresa): void {
    if (!this.podeExcluir) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para desativar usuários'
      });
      return;
    }

    if (usuario.ehProprietario) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Ação não permitida',
        detail: 'O usuário proprietário não pode ser desativado'
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Tem certeza que deseja desativar o usuário "${usuario.nome}"?`,
      header: 'Confirmar Desativação',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Sim',
      rejectLabel: 'Não',
      accept: () => {
        this.usuarioService.delete(usuario.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Sucesso',
              detail: 'Usuário desativado com sucesso'
            });
            this.loadUsuarios();
          },
          error: (error) => {
            console.error('Erro ao desativar usuário:', error);
            const errorMessage = error?.error?.message || 'Erro ao desativar usuário';
            this.messageService.add({
              severity: 'error',
              summary: 'Erro',
              detail: errorMessage
            });
          }
        });
      }
    });
  }

  mostrarDialogResetarSenha(usuario: UsuarioEmpresa): void {
    this.selectedUsuario = usuario;
    this.novaSenha = '';
    this.confirmSenha = '';
    this.displayResetarSenhaDialog = true;
  }

  resetarSenha(): void {
    if (!this.selectedUsuario) return;

    if (!this.podeResetarSenha) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Acesso Negado',
        detail: 'Você não tem permissão para resetar senhas'
      });
      return;
    }

    if (this.novaSenha !== this.confirmSenha) {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'As senhas não coincidem'
      });
      return;
    }

    if (this.novaSenha.length < 8) {
      this.messageService.add({
        severity: 'error',
        summary: 'Erro',
        detail: 'A senha deve ter no mínimo 8 caracteres'
      });
      return;
    }

    this.usuarioService.resetarSenha(this.selectedUsuario.id, { novaSenha: this.novaSenha }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Senha resetada com sucesso'
        });
        this.displayResetarSenhaDialog = false;
        this.selectedUsuario = null;
        this.novaSenha = '';
        this.confirmSenha = '';
      },
      error: (error) => {
        console.error('Erro ao resetar senha:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao resetar senha'
        });
      }
    });
  }

  getStatusClass(ativo: boolean): string {
    return ativo ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  }

  getStatusLabel(ativo: boolean): string {
    return ativo ? 'Ativo' : 'Inativo';
  }

  getPerfilLabel(usuario: UsuarioEmpresa): string {
    if (usuario.ehProprietario) {
      return 'Proprietário';
    }
    return usuario.perfilDescricao || this.getPerfilDescricaoPorEnum(usuario.perfil);
  }

  getPerfilDescricaoPorEnum(perfil: PerfilUsuario): string {
    switch (perfil) {
      case PerfilUsuario.Admin:
        return 'Administrador';
      case PerfilUsuario.Operador:
        return 'Operador';
      default:
        return 'Desconhecido';
    }
  }

  getPerfilClass(perfil: PerfilUsuario, ehProprietario: boolean = false): string {
    if (ehProprietario) {
      return 'bg-yellow-100 text-yellow-800';
    }
    switch (perfil) {
      case PerfilUsuario.Admin:
        return 'bg-purple-100 text-purple-800';
      case PerfilUsuario.Operador:
        return 'bg-blue-100 text-blue-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  carregarPermissoes(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.perfilUsuarioString = currentUser.perfil;

    // Verificar permissão de visualizar (read)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'usuarios',
      'read'
    ).subscribe({
      next: (response) => {
        this.podeVisualizar = response.permitido;
      },
      error: () => {
        this.podeVisualizar = false;
      }
    });

    // Verificar permissão de criar (create)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'usuarios',
      'create'
    ).subscribe({
      next: (response) => {
        this.podeCriar = response.permitido;
      },
      error: () => {
        this.podeCriar = false;
      }
    });

    // Verificar permissão de editar (update)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'usuarios',
      'update'
    ).subscribe({
      next: (response) => {
        this.podeEditar = response.permitido;
      },
      error: () => {
        this.podeEditar = false;
      }
    });

    // Verificar permissão de excluir (delete)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'usuarios',
      'delete'
    ).subscribe({
      next: (response) => {
        this.podeExcluir = response.permitido;
      },
      error: () => {
        this.podeExcluir = false;
      }
    });

    // Verificar permissão de resetar senha (reset-password)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'usuarios',
      'reset-password'
    ).subscribe({
      next: (response) => {
        this.podeResetarSenha = response.permitido;
      },
      error: () => {
        this.podeResetarSenha = false;
      }
    });
  }

  canCreate(): boolean {
    return this.podeCriar;
  }

  canEdit(): boolean {
    return this.podeEditar;
  }

  canDelete(): boolean {
    return this.podeExcluir;
  }

  canResetPassword(): boolean {
    return this.podeResetarSenha;
  }
}

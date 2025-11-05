import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UsuarioEmpresaService } from '../../../core/services/usuario-empresa.service';
import { PerfilUsuario, CreateUsuarioEmpresaRequest, UpdateUsuarioEmpresaRequest } from '../../../core/models';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-usuario-form',
  templateUrl: './usuario-form.component.html',
  styleUrls: ['./usuario-form.component.scss']
})
export class UsuarioFormComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  isEditMode = false;
  usuarioId?: string;
  showPassword = false;
  showConfirmPassword = false;

  perfisOptions = [
    {
      label: 'Administrador',
      value: PerfilUsuario.Admin,
      description: 'Acesso total ao sistema, exceto gerenciar outros Admins'
    },
    {
      label: 'Operador',
      value: PerfilUsuario.Operador,
      description: 'Acesso apenas para visualizar Regras de Cobrança'
    }
  ];

  constructor(
    private fb: FormBuilder,
    private usuarioService: UsuarioEmpresaService,
    private router: Router,
    private route: ActivatedRoute,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.checkEditMode();
  }

  initForm(): void {
    this.form = this.fb.group({
      nome: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email]],
      perfil: [null, Validators.required],
      senha: ['', [Validators.minLength(8)]],
      confirmSenha: ['']
    });
  }

  checkEditMode(): void {
    this.usuarioId = this.route.snapshot.params['id'];

    if (this.usuarioId) {
      this.isEditMode = true;
      // Em modo edição, senha não é obrigatória
      this.form.get('senha')?.clearValidators();
      this.form.get('senha')?.updateValueAndValidity();
      this.loadUsuario(this.usuarioId);
    } else {
      // Em modo criação, senha é obrigatória
      this.form.get('senha')?.setValidators([Validators.required, Validators.minLength(8)]);
      this.form.get('senha')?.updateValueAndValidity();
    }
  }

  loadUsuario(id: string): void {
    this.loading = true;
    this.usuarioService.getById(id).subscribe({
      next: (usuario) => {
        this.form.patchValue({
          nome: usuario.nome,
          email: usuario.email,
          perfil: usuario.perfil
        });
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar usuário:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar dados do usuário'
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
        severity: 'error',
        summary: 'Erro',
        detail: 'Por favor, preencha todos os campos obrigatórios corretamente'
      });
      return;
    }

    // Validar senhas apenas no modo de criação
    if (!this.isEditMode) {
      const senha = this.form.get('senha')?.value;
      const confirmSenha = this.form.get('confirmSenha')?.value;

      if (senha !== confirmSenha) {
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'As senhas não coincidem'
        });
        return;
      }
    }

    this.loading = true;

    if (this.isEditMode && this.usuarioId) {
      this.atualizar();
    } else {
      this.criar();
    }
  }

  criar(): void {
    const request: CreateUsuarioEmpresaRequest = {
      nome: this.form.get('nome')?.value,
      email: this.form.get('email')?.value,
      perfil: this.form.get('perfil')?.value,
      senha: this.form.get('senha')?.value
    };

    this.usuarioService.create(request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Usuário criado com sucesso'
        });
        this.loading = false;
        this.voltar();
      },
      error: (error) => {
        console.error('Erro ao criar usuário:', error);
        const errorMessage = error?.error?.message || 'Erro ao criar usuário';
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  atualizar(): void {
    if (!this.usuarioId) return;

    const request: UpdateUsuarioEmpresaRequest = {
      nome: this.form.get('nome')?.value,
      perfil: this.form.get('perfil')?.value,
      ativo: true // Sempre ativo por padrão, desativar é feito via botão na lista
    };

    this.usuarioService.update(this.usuarioId, request).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Usuário atualizado com sucesso'
        });
        this.loading = false;
        this.voltar();
      },
      error: (error) => {
        console.error('Erro ao atualizar usuário:', error);
        const errorMessage = error?.error?.message || 'Erro ao atualizar usuário';
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: errorMessage
        });
        this.loading = false;
      }
    });
  }

  voltar(): void {
    this.router.navigate(['/usuarios']);
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
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

  // Getters para facilitar acesso no template
  get nome() { return this.form.get('nome'); }
  get email() { return this.form.get('email'); }
  get perfil() { return this.form.get('perfil'); }
  get senha() { return this.form.get('senha'); }
  get confirmSenha() { return this.form.get('confirmSenha'); }
}

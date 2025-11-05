import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { PermissaoService } from '../../core/services/permissao.service';
import { AuthService } from '../../core/services/auth.service';
import { MessageService } from 'primeng/api';
import {
  Modulo,
  Acao,
  PermissaoPerfil,
  ModuloPermissao,
  AcaoPermissao,
  TipoAcao,
  ConfigurarPermissoesRequest
} from '../../core/models/permissao.models';
import { PerfilUsuario } from '../../core/models/usuario-empresa.models';

// PrimeNG Imports
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';

interface PerfilOption {
  label: string;
  value: PerfilUsuario;
  description: string;
}

interface AcaoEspecialInfo {
  chave: string;
  label: string;
  icone: string;
  descricao: string;
}

@Component({
  selector: 'app-permissoes',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    TableModule,
    ButtonModule,
    DropdownModule,
    CheckboxModule,
    ToastModule,
    ProgressSpinnerModule,
    DialogModule,
    TooltipModule
  ],
  providers: [MessageService],
  templateUrl: './permissoes.component.html',
  styleUrls: ['./permissoes.component.scss']
})
export class PermissoesComponent implements OnInit {
  loading = false;
  saving = false;

  perfilSelecionado: PerfilUsuario = PerfilUsuario.Admin;
  permissoes: PermissaoPerfil | null = null;
  permissoesOriginais: { [moduloId: string]: { [acaoId: string]: boolean } } = {};

  // Permissões
  perfilUsuarioString: string = '';
  podeVisualizar = false;

  perfisOptions: PerfilOption[] = [
    {
      label: 'Administrador',
      value: PerfilUsuario.Admin,
      description: 'Acesso completo ao sistema (exceto Permissões)'
    },
    {
      label: 'Operador',
      value: PerfilUsuario.Operador,
      description: 'Acesso limitado definido pelo proprietário'
    }
  ];

  // Estado temporário das permissões (modificado antes de salvar)
  permissoesEditadas: { [moduloId: string]: { [acaoId: string]: boolean } } = {};

  // Mapeamento de ações especiais com informações visuais
  acoesEspeciaisInfo: { [chave: string]: AcaoEspecialInfo } = {
    'toggle': {
      chave: 'toggle',
      label: 'Ativar/Desativar',
      icone: 'pi-power-off',
      descricao: 'Ativar ou desativar registros'
    },
    'export': {
      chave: 'export',
      label: 'Exportar',
      icone: 'pi-download',
      descricao: 'Exportar dados para arquivos'
    },
    'import': {
      chave: 'import',
      label: 'Importar',
      icone: 'pi-upload',
      descricao: 'Importar dados de arquivos'
    },
    'reset-password': {
      chave: 'reset-password',
      label: 'Resetar Senha',
      icone: 'pi-key',
      descricao: 'Resetar senha de usuários'
    },
    'config-permissions': {
      chave: 'config-permissions',
      label: 'Configurar',
      icone: 'pi-cog',
      descricao: 'Configurar permissões de perfis'
    }
  };

  constructor(
    private permissaoService: PermissaoService,
    private messageService: MessageService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.verificarPermissaoAcesso();
    this.carregarPermissoes();
  }

  carregarPermissoes(): void {
    this.loading = true;
    this.permissaoService.getPermissoesPorPerfil(this.perfilSelecionado).subscribe({
      next: (permissoes) => {
        this.permissoes = permissoes;
        this.inicializarPermissoesEditadas();
        this.loading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar permissões:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: 'Erro ao carregar permissões do perfil'
        });
        this.loading = false;
      }
    });
  }

  inicializarPermissoesEditadas(): void {
    this.permissoesEditadas = {};
    this.permissoesOriginais = {};
    if (!this.permissoes) return;

    this.permissoes.modulos.forEach(modulo => {
      this.permissoesEditadas[modulo.moduloId] = {};
      this.permissoesOriginais[modulo.moduloId] = {};
      modulo.acoes.forEach(acao => {
        this.permissoesEditadas[modulo.moduloId][acao.acaoId] = acao.permitido;
        this.permissoesOriginais[modulo.moduloId][acao.acaoId] = acao.permitido;
      });
    });
  }

  onPerfilChange(): void {
    this.carregarPermissoes();
  }

  togglePermissao(moduloId: string, acaoId: string): void {
    if (!this.permissoesEditadas[moduloId]) {
      this.permissoesEditadas[moduloId] = {};
    }
    this.permissoesEditadas[moduloId][acaoId] = !this.permissoesEditadas[moduloId][acaoId];
  }

  isPermitido(moduloId: string, acaoId: string): boolean {
    return this.permissoesEditadas[moduloId]?.[acaoId] ?? false;
  }

  toggleTodasAcoesModulo(modulo: ModuloPermissao): void {
    const todasPermitidas = modulo.acoes.every(acao =>
      this.isPermitido(modulo.moduloId, acao.acaoId)
    );

    modulo.acoes.forEach(acao => {
      if (!this.permissoesEditadas[modulo.moduloId]) {
        this.permissoesEditadas[modulo.moduloId] = {};
      }
      this.permissoesEditadas[modulo.moduloId][acao.acaoId] = !todasPermitidas;
    });
  }

  todasAcoesPermitidas(modulo: ModuloPermissao): boolean {
    return modulo.acoes.every(acao => this.isPermitido(modulo.moduloId, acao.acaoId));
  }

  salvarPermissoes(): void {
    this.saving = true;

    const request: ConfigurarPermissoesRequest = {
      perfilUsuario: this.perfilSelecionado,
      permissoes: this.permissoesEditadas
    };

    this.permissaoService.configurarPermissoes(request).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Sucesso',
          detail: 'Permissões configuradas com sucesso'
        });
        this.saving = false;
        this.carregarPermissoes();
      },
      error: (error) => {
        console.error('Erro ao salvar permissões:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Erro',
          detail: error.error?.message || 'Erro ao salvar permissões'
        });
        this.saving = false;
      }
    });
  }

  cancelar(): void {
    this.inicializarPermissoesEditadas();
    this.messageService.add({
      severity: 'info',
      summary: 'Cancelado',
      detail: 'Alterações descartadas'
    });
  }

  // Métodos auxiliares para a tabela matricial

  /**
   * Retorna uma ação específica por chave (menu.view, read, create, update, delete)
   */
  getAcaoPorChave(modulo: ModuloPermissao, chave: string): AcaoPermissao | null {
    return modulo.acoes.find(a => a.acaoChave === chave) || null;
  }

  /**
   * Retorna todas as ações especiais únicas do sistema (não CRUD básicas)
   */
  getAcoesEspeciaisUnicas(): AcaoEspecialInfo[] {
    if (!this.permissoes) return [];

    const acoesBasicas = ['menu.view', 'read', 'create', 'update', 'delete'];
    const acoesUnicas = new Set<string>();

    // Coletar todas as ações especiais únicas de todos os módulos
    this.permissoes.modulos.forEach(modulo => {
      modulo.acoes.forEach(acao => {
        if (!acoesBasicas.includes(acao.acaoChave)) {
          acoesUnicas.add(acao.acaoChave);
        }
      });
    });

    // Retornar informações formatadas para cada ação especial
    return Array.from(acoesUnicas)
      .map(chave => this.acoesEspeciaisInfo[chave])
      .filter(info => info !== undefined); // Remove undefined se houver ações não mapeadas
  }

  /**
   * Verifica se há mudanças pendentes não salvas
   */
  temMudancasPendentes(): boolean {
    if (!this.permissoes) return false;

    for (const modulo of this.permissoes.modulos) {
      for (const acao of modulo.acoes) {
        const valorOriginal = this.permissoesOriginais[modulo.moduloId]?.[acao.acaoId] ?? false;
        const valorEditado = this.permissoesEditadas[modulo.moduloId]?.[acao.acaoId] ?? false;
        if (valorOriginal !== valorEditado) {
          return true;
        }
      }
    }

    return false;
  }

  /**
   * Conta quantas mudanças pendentes existem
   */
  getCountMudancasPendentes(): number {
    if (!this.permissoes) return 0;

    let count = 0;
    for (const modulo of this.permissoes.modulos) {
      for (const acao of modulo.acoes) {
        const valorOriginal = this.permissoesOriginais[modulo.moduloId]?.[acao.acaoId] ?? false;
        const valorEditado = this.permissoesEditadas[modulo.moduloId]?.[acao.acaoId] ?? false;
        if (valorOriginal !== valorEditado) {
          count++;
        }
      }
    }

    return count;
  }

  /**
   * Verifica se um módulo específico tem mudanças
   */
  moduloTemMudancas(moduloId: string): boolean {
    const moduloOriginal = this.permissoesOriginais[moduloId];
    const moduloEditado = this.permissoesEditadas[moduloId];

    if (!moduloOriginal || !moduloEditado) return false;

    for (const acaoId in moduloOriginal) {
      if (moduloOriginal[acaoId] !== moduloEditado[acaoId]) {
        return true;
      }
    }

    return false;
  }

  /**
   * Verifica se o usuário tem permissão para acessar o módulo de Permissões
   */
  verificarPermissaoAcesso(): void {
    const currentUser = this.authService.currentUserValue;
    if (!currentUser) {
      this.router.navigate(['/auth/login']);
      return;
    }

    this.perfilUsuarioString = currentUser.perfil;

    // Verificar permissão de visualizar (read)
    this.permissaoService.verificarPermissao(
      this.perfilUsuarioString,
      'permissoes',
      'read'
    ).subscribe({
      next: (response) => {
        this.podeVisualizar = response.permitido;
      },
      error: () => {
        this.podeVisualizar = false;
        this.router.navigate(['/dashboard']);
      }
    });
  }
}

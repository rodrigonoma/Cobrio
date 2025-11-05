import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissaoService } from '../../core/services/permissao.service';
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

  // Dialog de ações especiais
  mostrarDialogAcoesEspeciais = false;
  moduloSelecionadoDialog: ModuloPermissao | null = null;

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

  constructor(
    private permissaoService: PermissaoService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
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
    if (!this.permissoes) return;

    this.permissoes.modulos.forEach(modulo => {
      this.permissoesEditadas[modulo.moduloId] = {};
      modulo.acoes.forEach(acao => {
        this.permissoesEditadas[modulo.moduloId][acao.acaoId] = acao.permitido;
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
   * Retorna todas as ações especiais (que não são CRUD básicas nem menu.view)
   */
  getAcoesEspeciais(modulo: ModuloPermissao): AcaoPermissao[] {
    const acoesBasicas = ['menu.view', 'read', 'create', 'update', 'delete'];
    return modulo.acoes.filter(a => !acoesBasicas.includes(a.acaoChave));
  }

  /**
   * Conta quantas ações especiais estão ativas para um módulo
   */
  getCountAcoesEspeciaisAtivas(modulo: ModuloPermissao): number {
    return this.getAcoesEspeciais(modulo).filter(acao =>
      this.isPermitido(modulo.moduloId, acao.acaoId)
    ).length;
  }

  /**
   * Mostra dialog com ações especiais de um módulo
   */
  mostrarAcoesEspeciais(modulo: ModuloPermissao): void {
    this.moduloSelecionadoDialog = modulo;
    this.mostrarDialogAcoesEspeciais = true;
  }
}

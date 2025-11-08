import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PermissaoService } from '../../../core/services/permissao.service';
import { AuthService } from '../../../core/services/auth.service';
import { RelatoriosOperacionaisComponent } from '../../relatorios-operacionais/relatorios-operacionais.component';
import { RelatoriosGerenciaisComponent } from '../../relatorios-gerenciais/relatorios-gerenciais.component';

interface TabItem {
  label: string;
  moduloChave: string;
  component: any;
  visible: boolean;
}

@Component({
  selector: 'app-relatorios-avancados',
  standalone: true,
  imports: [CommonModule, RelatoriosOperacionaisComponent, RelatoriosGerenciaisComponent],
  templateUrl: './relatorios-avancados.component.html',
  styleUrls: ['./relatorios-avancados.component.css']
})
export class RelatoriosAvancadosComponent implements OnInit {
  activeTabIndex = 0;
  userProfile: string = '';

  tabs: TabItem[] = [
    {
      label: 'Relatórios Operacionais',
      moduloChave: 'relatorios-operacionais',
      component: RelatoriosOperacionaisComponent,
      visible: false
    },
    {
      label: 'Relatórios Gerenciais',
      moduloChave: 'relatorios-gerenciais',
      component: RelatoriosGerenciaisComponent,
      visible: false
    }
  ];

  constructor(
    private permissaoService: PermissaoService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user?.perfil) {
        this.userProfile = user.perfil;
        this.verificarPermissoes();
      }
    });
  }

  verificarPermissoes(): void {
    console.log('[RelatoriosAvancados] Verificando permissões para perfil:', this.userProfile);

    // Verificar permissão para cada aba
    this.tabs.forEach((tab, index) => {
      console.log(`[RelatoriosAvancados] Verificando permissão para: ${tab.moduloChave}`);

      this.permissaoService.verificarPermissao(
        this.userProfile,
        tab.moduloChave,
        'read'
      ).subscribe({
        next: (result) => {
          console.log(`[RelatoriosAvancados] ${tab.moduloChave}: permitido=${result.permitido}`);
          tab.visible = result.permitido;

          // Se a aba ativa não tem permissão, mudar para a primeira aba visível
          if (this.activeTabIndex === index && !tab.visible) {
            this.activeTabIndex = this.tabs.findIndex(t => t.visible);
          }
        },
        error: (err) => {
          console.error(`Erro ao verificar permissão para ${tab.label}:`, err);
          tab.visible = false;
        }
      });
    });
  }

  selectTab(index: number): void {
    if (this.tabs[index].visible) {
      this.activeTabIndex = index;
    }
  }

  isTabVisible(index: number): boolean {
    return this.tabs[index].visible;
  }

  hasAnyPermission(): boolean {
    return this.tabs.some(tab => tab.visible);
  }

  getActiveTabLabel(): string {
    return this.tabs[this.activeTabIndex]?.label || '';
  }
}

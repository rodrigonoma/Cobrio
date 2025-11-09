import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthService } from '../../../core/services/auth.service';
import { PermissaoService } from '../../../core/services/permissao.service';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  moduloChave: string; // Chave do módulo no sistema de permissões
  visible: boolean; // Visibilidade dinâmica baseada em permissões
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() collapsed = false;
  private destroy$ = new Subject<void>();
  userProfile: string = '';

  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'pi-home', route: '/dashboard', moduloChave: 'dashboard', visible: false },
    { label: 'Assinaturas', icon: 'pi-users', route: '/assinaturas', moduloChave: 'assinaturas', visible: false },
    { label: 'Planos', icon: 'pi-tag', route: '/planos', moduloChave: 'planos', visible: false },
    { label: 'Regras de Cobrança', icon: 'pi-bell', route: '/regras-cobranca', moduloChave: 'regras-cobranca', visible: false },
    { label: 'Usuários', icon: 'pi-user-edit', route: '/usuarios', moduloChave: 'usuarios', visible: false },
    { label: 'Templates', icon: 'pi-file', route: '/templates', moduloChave: 'templates', visible: false },
    { label: 'Relatórios', icon: 'pi-chart-bar', route: '/relatorios', moduloChave: 'relatorios', visible: false },
    { label: 'Permissões', icon: 'pi-shield', route: '/permissoes', moduloChave: 'permissoes', visible: false },
    { label: 'Configurações', icon: 'pi-cog', route: '/configuracoes/email', moduloChave: 'configuracoes', visible: false }
  ];

  constructor(
    public router: Router,
    private authService: AuthService,
    private permissaoService: PermissaoService
  ) { }

  ngOnInit(): void {
    // Obter perfil do usuário e carregar permissões
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        if (user?.perfil) {
          this.userProfile = user.perfil;
          this.carregarPermissoesMenu();
        }
      });
  }

  carregarPermissoesMenu(): void {
    console.log('[Sidebar] Carregando permissões para perfil:', this.userProfile);

    // Criar array de observables para verificar permissão de cada menu
    const permissaoChecks = this.menuItems.map(item => {
      console.log('[Sidebar] Verificando permissão para:', item.moduloChave);

      // Para o menu de Relatórios, verificar se tem permissão em qualquer submódulo
      if (item.moduloChave === 'relatorios') {
        return forkJoin([
          this.permissaoService.verificarPermissao(this.userProfile, 'relatorios', 'menu.view'),
          this.permissaoService.verificarPermissao(this.userProfile, 'relatorios-operacionais', 'read'),
          this.permissaoService.verificarPermissao(this.userProfile, 'relatorios-gerenciais', 'read')
        ]).pipe(
          map((results: any[]) => ({
            permitido: results[0].permitido || results[1].permitido || results[2].permitido
          }))
        );
      }

      return this.permissaoService.verificarPermissao(this.userProfile, item.moduloChave, 'menu.view');
    });

    // Executar todas as verificações em paralelo
    forkJoin(permissaoChecks).subscribe({
      next: (results) => {
        console.log('[Sidebar] Resultados das permissões:', results);
        results.forEach((result, index) => {
          this.menuItems[index].visible = result.permitido;
          console.log(`[Sidebar] ${this.menuItems[index].label}: ${result.permitido}`);
        });
      },
      error: (err) => {
        console.error('[Sidebar] Erro ao carregar permissões do menu:', err);
        // Em caso de erro, manter todos invisíveis por segurança
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  isActive(route: string): boolean {
    return this.router.url === route;
  }

  canViewMenuItem(item: MenuItem): boolean {
    return item.visible;
  }
}

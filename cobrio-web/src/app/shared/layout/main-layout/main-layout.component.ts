import { Component, OnInit, HostListener } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss'],
  animations: [
    trigger('fadeAnimation', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-in-out', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in-out', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class MainLayoutComponent implements OnInit {
  sidebarCollapsed = false; // Desktop: aberto, Mobile: controlado no ngOnInit

  ngOnInit(): void {
    // Inicializar estado baseado no tamanho da tela
    this.sidebarCollapsed = window.innerWidth < 768;
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;

    // Prevenir scroll do body quando sidebar aberta em mobile
    if (!this.sidebarCollapsed && window.innerWidth < 768) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  closeSidebar(): void {
    this.sidebarCollapsed = true;
    document.body.style.overflow = '';
  }

  // Fechar sidebar ao pressionar ESC
  @HostListener('document:keydown.escape')
  onEscapePress(): void {
    if (!this.sidebarCollapsed && window.innerWidth < 768) {
      this.closeSidebar();
    }
  }

  // Ajustar sidebar ao redimensionar
  @HostListener('window:resize')
  onResize(): void {
    const isMobile = window.innerWidth < 768;

    if (!isMobile) {
      // Desktop: abrir sidebar e limpar overflow
      this.sidebarCollapsed = false;
      document.body.style.overflow = '';
    } else {
      // Mobile: fechar sidebar
      this.sidebarCollapsed = true;
      document.body.style.overflow = '';
    }
  }
}

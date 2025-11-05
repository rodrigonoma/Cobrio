import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService, Notification } from '../../../core/services/notification.service';
import { Usuario } from '../../../core/models';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  @Output() toggleSidebar = new EventEmitter<void>();

  currentUser: Usuario | null = null;
  userMenuVisible = false;
  notificationPanelVisible = false;
  notifications: Notification[] = [];
  unreadCount = 0;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });

    // Subscreve às notificações
    this.notificationService.getNotifications().subscribe(notifications => {
      this.notifications = notifications;
    });

    this.notificationService.getUnreadCount().subscribe(count => {
      this.unreadCount = count;
    });
  }

  onToggleSidebar(): void {
    this.toggleSidebar.emit();
  }

  toggleUserMenu(): void {
    this.userMenuVisible = !this.userMenuVisible;
  }

  onLogout(): void {
    this.authService.logout();
    this.userMenuVisible = false;
  }

  getUserInitials(): string {
    if (!this.currentUser?.nome) return '?';
    const names = this.currentUser.nome.split(' ');
    if (names.length >= 2) {
      return `${names[0][0]}${names[1][0]}`.toUpperCase();
    }
    return this.currentUser.nome.substring(0, 2).toUpperCase();
  }

  toggleNotificationPanel(): void {
    this.notificationPanelVisible = !this.notificationPanelVisible;
    if (this.notificationPanelVisible) {
      this.userMenuVisible = false;
    }
  }

  markNotificationAsRead(notification: Notification): void {
    this.notificationService.markAsRead(notification.id);
  }

  markAllNotificationsAsRead(): void {
    this.notificationService.markAllAsRead();
  }

  clearAllNotifications(): void {
    this.notificationService.clearAll();
    this.notificationPanelVisible = false;
  }

  getNotificationIcon(type: Notification['type']): string {
    const icons = {
      'trial-expiring': 'pi-clock',
      'payment-overdue': 'pi-exclamation-circle',
      'subscription-cancelled': 'pi-times-circle',
      'payment-received': 'pi-check-circle',
      'info': 'pi-info-circle'
    };
    return icons[type] || 'pi-bell';
  }

  getNotificationTime(timestamp: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(timestamp).getTime();
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'agora';
    if (minutes < 60) return `${minutes}m atrás`;
    if (hours < 24) return `${hours}h atrás`;
    return `${days}d atrás`;
  }
}

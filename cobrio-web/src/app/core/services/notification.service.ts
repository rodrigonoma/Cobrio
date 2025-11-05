import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { AssinaturaService } from './assinatura.service';
import { Assinatura } from '../models/assinatura.models';

export interface Notification {
  id: string;
  type: 'trial-expiring' | 'payment-overdue' | 'subscription-cancelled' | 'payment-received' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  read: boolean;
  assinaturaId?: string;
  severity: 'success' | 'info' | 'warning' | 'danger';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);
  private notificationIdCounter = 1;

  constructor(private assinaturaService: AssinaturaService) {
    this.initializeNotifications();
  }

  getNotifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  getUnreadCount(): Observable<number> {
    return new Observable(observer => {
      this.notifications$.subscribe(notifications => {
        const unreadCount = notifications.filter(n => !n.read).length;
        observer.next(unreadCount);
      });
    });
  }

  markAsRead(notificationId: string): void {
    const notifications = this.notifications$.value.map(n =>
      n.id === notificationId ? { ...n, read: true } : n
    );
    this.notifications$.next(notifications);
  }

  markAllAsRead(): void {
    const notifications = this.notifications$.value.map(n => ({ ...n, read: true }));
    this.notifications$.next(notifications);
  }

  clearAll(): void {
    this.notifications$.next([]);
  }

  private initializeNotifications(): void {
    // Carrega assinaturas e gera notificações
    this.assinaturaService.getAll().subscribe({
      next: (assinaturas) => {
        this.generateNotificationsFromAssinaturas(assinaturas);

        // Atualiza notificações a cada 5 minutos
        setInterval(() => {
          this.assinaturaService.getAll().subscribe({
            next: (updatedAssinaturas) => {
              this.generateNotificationsFromAssinaturas(updatedAssinaturas);
            }
          });
        }, 5 * 60 * 1000); // 5 minutos
      }
    });
  }

  private generateNotificationsFromAssinaturas(assinaturas: Assinatura[]): void {
    const newNotifications: Notification[] = [];
    const hoje = new Date();

    assinaturas.forEach(assinatura => {
      // Notificações de trial expirando
      if (assinatura.emTrial && assinatura.trialFim) {
        const trialFim = new Date(assinatura.trialFim);
        const diasRestantes = Math.ceil((trialFim.getTime() - hoje.getTime()) / (1000 * 60 * 60 * 24));

        if (diasRestantes === 7) {
          newNotifications.push(this.createNotification({
            type: 'trial-expiring',
            title: 'Trial expirando em 7 dias',
            message: `O trial de ${assinatura.assinanteNome} (${assinatura.planoNome}) expira em 7 dias.`,
            severity: 'info',
            assinaturaId: assinatura.id
          }));
        } else if (diasRestantes === 3) {
          newNotifications.push(this.createNotification({
            type: 'trial-expiring',
            title: 'Trial expirando em 3 dias',
            message: `O trial de ${assinatura.assinanteNome} (${assinatura.planoNome}) expira em 3 dias.`,
            severity: 'warning',
            assinaturaId: assinatura.id
          }));
        } else if (diasRestantes === 1) {
          newNotifications.push(this.createNotification({
            type: 'trial-expiring',
            title: 'Trial expirando amanhã!',
            message: `O trial de ${assinatura.assinanteNome} (${assinatura.planoNome}) expira amanhã.`,
            severity: 'danger',
            assinaturaId: assinatura.id
          }));
        }
      }

      // Notificações de pagamento vencido
      if (assinatura.status === 'Ativa' && assinatura.dataProximaCobranca) {
        const proximaCobranca = new Date(assinatura.dataProximaCobranca);
        const diasVencido = Math.floor((hoje.getTime() - proximaCobranca.getTime()) / (1000 * 60 * 60 * 24));

        if (diasVencido > 0 && diasVencido <= 5) {
          newNotifications.push(this.createNotification({
            type: 'payment-overdue',
            title: 'Pagamento vencido',
            message: `Pagamento de ${assinatura.assinanteNome} (${assinatura.planoNome}) está vencido há ${diasVencido} dia(s).`,
            severity: 'danger',
            assinaturaId: assinatura.id
          }));
        }
      }

      // Notificações de assinatura cancelada (recente)
      if (assinatura.status === 'Cancelada' && assinatura.dataCancelamento) {
        const dataCancelamento = new Date(assinatura.dataCancelamento);
        const diasDesde = Math.floor((hoje.getTime() - dataCancelamento.getTime()) / (1000 * 60 * 60 * 24));

        if (diasDesde <= 1) { // Cancelada nas últimas 24h
          newNotifications.push(this.createNotification({
            type: 'subscription-cancelled',
            title: 'Assinatura cancelada',
            message: `${assinatura.assinanteNome} cancelou a assinatura do plano ${assinatura.planoNome}.`,
            severity: 'warning',
            assinaturaId: assinatura.id
          }));
        }
      }
    });

    // Mescla com notificações existentes (evita duplicatas)
    const existingNotifications = this.notifications$.value;
    const mergedNotifications = [...existingNotifications];

    newNotifications.forEach(newNotif => {
      // Verifica se já existe uma notificação similar
      const exists = existingNotifications.some(existing =>
        existing.assinaturaId === newNotif.assinaturaId &&
        existing.type === newNotif.type &&
        existing.title === newNotif.title
      );

      if (!exists) {
        mergedNotifications.unshift(newNotif); // Adiciona no início
      }
    });

    // Limita a 50 notificações
    const limitedNotifications = mergedNotifications.slice(0, 50);
    this.notifications$.next(limitedNotifications);
  }

  private createNotification(params: {
    type: Notification['type'];
    title: string;
    message: string;
    severity: Notification['severity'];
    assinaturaId?: string;
  }): Notification {
    return {
      id: `notif-${this.notificationIdCounter++}`,
      type: params.type,
      title: params.title,
      message: params.message,
      timestamp: new Date(),
      read: false,
      severity: params.severity,
      assinaturaId: params.assinaturaId
    };
  }

  // Método para adicionar notificação manual
  addNotification(
    type: Notification['type'],
    title: string,
    message: string,
    severity: Notification['severity'] = 'info'
  ): void {
    const notification = this.createNotification({ type, title, message, severity });
    const notifications = [notification, ...this.notifications$.value];
    this.notifications$.next(notifications.slice(0, 50));
  }
}

import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import {
  LoginRequest,
  LoginResponse,
  RefreshTokenResponse,
  Usuario
} from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<Usuario | null>;
  public currentUser$: Observable<Usuario | null>;

  constructor(
    private api: ApiService,
    private storage: StorageService,
    private router: Router
  ) {
    const user = this.storage.getItem<Usuario>(environment.userKey);
    console.log('[AuthService] Usuário carregado do localStorage:', user);
    this.currentUserSubject = new BehaviorSubject<Usuario | null>(user);
    this.currentUser$ = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): Usuario | null {
    return this.currentUserSubject.value;
  }

  public get isAuthenticated(): boolean {
    return !!this.getToken();
  }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.api.post<LoginResponse>('Auth/login', request).pipe(
      tap(response => {
        console.log('[AuthService] Login realizado. Usuário:', response.user);
        this.storage.setItem(environment.tokenKey, response.accessToken);
        this.storage.setItem(environment.refreshTokenKey, response.refreshToken);
        this.storage.setItem(environment.userKey, response.user);
        this.currentUserSubject.next(response.user);
        console.log('[AuthService] Usuário salvo no localStorage e BehaviorSubject atualizado');
      })
    );
  }

  refreshToken(): Observable<RefreshTokenResponse> {
    const refreshToken = this.getRefreshToken();
    return this.api.post<RefreshTokenResponse>('Auth/refresh', { RefreshToken: refreshToken }).pipe(
      tap(response => {
        this.storage.setItem(environment.tokenKey, response.accessToken);
        this.storage.setItem(environment.refreshTokenKey, response.refreshToken);
      })
    );
  }

  revokeToken(): Observable<void> {
    const refreshToken = this.getRefreshToken();
    return this.api.post<void>('Auth/revoke', { RefreshToken: refreshToken });
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();

    // Só tenta revogar se houver um refresh token válido
    if (refreshToken) {
      this.revokeToken().subscribe({
        next: () => {
          console.log('[AuthService] Token revogado com sucesso');
          this.clearSession();
        },
        error: (err) => {
          console.log('[AuthService] Erro ao revogar token (ignorado):', err);
          // Mesmo com erro, limpa a sessão
          this.clearSession();
        }
      });
    } else {
      // Se não houver refresh token, apenas limpa a sessão
      this.clearSession();
    }
  }

  private clearSession(): void {
    console.log('[AuthService] Limpando sessão e redirecionando para login');
    this.storage.removeItem(environment.tokenKey);
    this.storage.removeItem(environment.refreshTokenKey);
    this.storage.removeItem(environment.userKey);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.storage.getItem<string>(environment.tokenKey);
  }

  getRefreshToken(): string | null {
    return this.storage.getItem<string>(environment.refreshTokenKey);
  }

  validateToken(): Observable<any> {
    return this.api.get('Auth/validate');
  }
}

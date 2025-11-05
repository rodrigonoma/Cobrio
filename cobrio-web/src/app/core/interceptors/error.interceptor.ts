import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          return this.handle401Error(request, next);
        }

        if (error.status === 403) {
          this.router.navigate(['/acesso-negado']);
        }

        return throwError(() => this.formatError(error));
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Não tentar refresh em endpoints de autenticação
    const isAuthEndpoint = request.url.includes('/refresh') ||
                          request.url.includes('/login') ||
                          request.url.includes('/Auth/login') ||
                          request.url.includes('/revoke') ||
                          request.url.includes('/Auth/revoke');

    if (isAuthEndpoint) {
      // Se for login, apenas retorna o erro sem fazer logout
      if (request.url.includes('/login') || request.url.includes('/Auth/login')) {
        return throwError(() => new Error('Credenciais inválidas'));
      }

      // Se for revoke ou refresh que falhou, apenas retorna erro (logout já está tratando isso)
      if (request.url.includes('/revoke') || request.url.includes('/Auth/revoke')) {
        return throwError(() => new Error('Erro ao revogar token'));
      }

      // Se for refresh que falhou, faz logout
      this.authService.logout();
      return throwError(() => new Error('Sessão expirada'));
    }

    // Verifica se tem refresh token antes de tentar refresh
    const refreshToken = this.authService.getRefreshToken();
    if (!refreshToken) {
      this.authService.logout();
      return throwError(() => new Error('Sessão expirada'));
    }

    return this.authService.refreshToken().pipe(
      switchMap(() => {
        const token = this.authService.getToken();
        const clonedRequest = request.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next.handle(clonedRequest);
      }),
      catchError(err => {
        this.authService.logout();
        return throwError(() => err);
      })
    );
  }

  private formatError(error: HttpErrorResponse): any {
    if (error.error instanceof ErrorEvent) {
      return {
        message: `Erro: ${error.error.message}`,
        statusCode: 0
      };
    }

    // Se o backend retornou um objeto de erro estruturado, preservá-lo
    if (error.error && typeof error.error === 'object') {
      return {
        message: error.error?.message || error.error?.mensagem || error.message || 'Erro desconhecido',
        error: error.error, // Preserva o objeto original do backend
        errors: error.error?.errors,
        statusCode: error.status
      };
    }

    return {
      message: error.message || 'Erro desconhecido',
      statusCode: error.status
    };
  }
}

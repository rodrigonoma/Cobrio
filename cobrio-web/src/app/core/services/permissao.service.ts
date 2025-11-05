import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Modulo,
  Acao,
  PermissaoPerfil,
  ConfigurarPermissoesRequest
} from '../models/permissao.models';
import { PerfilUsuario } from '../models/usuario-empresa.models';

@Injectable({
  providedIn: 'root'
})
export class PermissaoService {
  private apiUrl = `${environment.apiUrl}/permissoes`;

  constructor(private http: HttpClient) {}

  getModulos(): Observable<Modulo[]> {
    return this.http.get<Modulo[]>(`${this.apiUrl}/modulos`);
  }

  getAcoes(): Observable<Acao[]> {
    return this.http.get<Acao[]>(`${this.apiUrl}/acoes`);
  }

  getPermissoesPorPerfil(perfil: PerfilUsuario): Observable<PermissaoPerfil> {
    return this.http.get<PermissaoPerfil>(`${this.apiUrl}/perfil/${perfil}`);
  }

  configurarPermissoes(request: ConfigurarPermissoesRequest): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/configurar`, request);
  }

  verificarPermissao(
    perfil: PerfilUsuario | string,
    moduloChave: string,
    acaoChave: string
  ): Observable<{ permitido: boolean }> {
    return this.http.get<{ permitido: boolean }>(`${this.apiUrl}/verificar`, {
      params: {
        perfil: typeof perfil === 'string' ? perfil : PerfilUsuario[perfil],
        moduloChave,
        acaoChave
      }
    });
  }
}

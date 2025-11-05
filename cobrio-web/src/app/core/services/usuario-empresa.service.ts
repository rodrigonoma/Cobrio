import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  UsuarioEmpresa,
  CreateUsuarioEmpresaRequest,
  UpdateUsuarioEmpresaRequest,
  ResetarSenhaRequest
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class UsuarioEmpresaService {

  constructor(private api: ApiService) { }

  getAll(): Observable<UsuarioEmpresa[]> {
    return this.api.get<UsuarioEmpresa[]>('UsuarioEmpresa');
  }

  getById(id: string): Observable<UsuarioEmpresa> {
    return this.api.get<UsuarioEmpresa>(`UsuarioEmpresa/${id}`);
  }

  create(request: CreateUsuarioEmpresaRequest): Observable<UsuarioEmpresa> {
    return this.api.post<UsuarioEmpresa>('UsuarioEmpresa', request);
  }

  update(id: string, request: UpdateUsuarioEmpresaRequest): Observable<UsuarioEmpresa> {
    return this.api.put<UsuarioEmpresa>(`UsuarioEmpresa/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`UsuarioEmpresa/${id}`);
  }

  resetarSenha(id: string, request: ResetarSenhaRequest): Observable<void> {
    return this.api.post<void>(`UsuarioEmpresa/${id}/resetar-senha`, request);
  }
}

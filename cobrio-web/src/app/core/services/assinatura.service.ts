import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  Assinatura,
  CreateAssinaturaRequest,
  UpdateAssinaturaRequest,
  CancelarAssinaturaRequest
} from '../models/assinatura.models';

@Injectable({
  providedIn: 'root'
})
export class AssinaturaService {

  constructor(private api: ApiService) { }

  getAll(): Observable<Assinatura[]> {
    return this.api.get<Assinatura[]>('Assinatura');
  }

  getById(id: string): Observable<Assinatura> {
    return this.api.get<Assinatura>(`Assinatura/${id}`);
  }

  create(request: CreateAssinaturaRequest): Observable<Assinatura> {
    return this.api.post<Assinatura>('Assinatura', request);
  }

  update(id: string, request: UpdateAssinaturaRequest): Observable<Assinatura> {
    return this.api.put<Assinatura>(`Assinatura/${id}`, request);
  }

  cancelar(id: string, request: CancelarAssinaturaRequest): Observable<void> {
    return this.api.post<void>(`Assinatura/${id}/cancelar`, request);
  }

  suspender(id: string): Observable<void> {
    return this.api.post<void>(`Assinatura/${id}/suspender`, {});
  }

  reativar(id: string): Observable<void> {
    return this.api.post<void>(`Assinatura/${id}/reativar`, {});
  }

  alterarPlano(id: string, novoPlanoId: string): Observable<void> {
    return this.api.post<void>(`Assinatura/${id}/alterar-plano`, { novoPlanoId });
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`Assinatura/${id}`);
  }
}

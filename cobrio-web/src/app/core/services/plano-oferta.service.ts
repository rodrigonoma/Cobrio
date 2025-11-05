import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { PlanoOferta, CreatePlanoOfertaRequest, UpdatePlanoOfertaRequest } from '../models/plano-oferta.models';

@Injectable({
  providedIn: 'root'
})
export class PlanoOfertaService {

  constructor(private api: ApiService) { }

  getAll(): Observable<PlanoOferta[]> {
    return this.api.get<PlanoOferta[]>('PlanoOferta');
  }

  getById(id: string): Observable<PlanoOferta> {
    return this.api.get<PlanoOferta>(`PlanoOferta/${id}`);
  }

  create(request: CreatePlanoOfertaRequest): Observable<PlanoOferta> {
    return this.api.post<PlanoOferta>('PlanoOferta', request);
  }

  update(id: string, request: UpdatePlanoOfertaRequest): Observable<PlanoOferta> {
    return this.api.put<PlanoOferta>(`PlanoOferta/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`PlanoOferta/${id}`);
  }

  activate(id: string): Observable<void> {
    return this.api.post<void>(`PlanoOferta/${id}/activate`, {});
  }

  deactivate(id: string): Observable<void> {
    return this.api.post<void>(`PlanoOferta/${id}/deactivate`, {});
  }
}

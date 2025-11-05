import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  RegraCobranca,
  CreateRegraCobrancaRequest,
  UpdateRegraCobrancaRequest,
  ImportacaoResultado
} from '../models/regra-cobranca.models';
import { HistoricoImportacao } from '../models/historico-importacao.models';
import { CreateCobrancaRequest } from '../models/cobranca.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class RegraCobrancaService {

  constructor(
    private api: ApiService,
    private http: HttpClient
  ) { }

  getAll(apenasAtivas?: boolean): Observable<RegraCobranca[]> {
    const params = apenasAtivas !== undefined ? { apenasAtivas: apenasAtivas.toString() } : {};
    return this.api.get<RegraCobranca[]>('RegraCobranca', params);
  }

  getById(id: string): Observable<RegraCobranca> {
    return this.api.get<RegraCobranca>(`RegraCobranca/${id}`);
  }

  create(request: CreateRegraCobrancaRequest): Observable<RegraCobranca> {
    return this.api.post<RegraCobranca>('RegraCobranca', request);
  }

  update(id: string, request: UpdateRegraCobrancaRequest): Observable<RegraCobranca> {
    return this.api.put<RegraCobranca>(`RegraCobranca/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`RegraCobranca/${id}`);
  }

  ativar(id: string): Observable<RegraCobranca> {
    return this.api.post<RegraCobranca>(`RegraCobranca/${id}/ativar`, {});
  }

  desativar(id: string): Observable<RegraCobranca> {
    return this.api.post<RegraCobranca>(`RegraCobranca/${id}/desativar`, {});
  }

  regenerarToken(id: string): Observable<RegraCobranca> {
    return this.api.post<RegraCobranca>(`RegraCobranca/${id}/regenerar-token`, {});
  }

  baixarModeloExcel(id: string): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/RegraCobranca/${id}/modelo-excel`, {
      responseType: 'blob'
    });
  }

  importarExcel(id: string, file: File): Observable<ImportacaoResultado> {
    const formData = new FormData();
    formData.append('arquivo', file, file.name);

    return this.http.post<ImportacaoResultado>(
      `${environment.apiUrl}/RegraCobranca/${id}/importar-excel`,
      formData
    );
  }

  importarJson(id: string, cobrancas: CreateCobrancaRequest[]): Observable<ImportacaoResultado> {
    return this.http.post<ImportacaoResultado>(
      `${environment.apiUrl}/RegraCobranca/${id}/importar-json`,
      cobrancas
    );
  }

  getHistoricoImportacoes(id: string): Observable<HistoricoImportacao[]> {
    return this.api.get<HistoricoImportacao[]>(`RegraCobranca/${id}/historico-importacoes`);
  }
}

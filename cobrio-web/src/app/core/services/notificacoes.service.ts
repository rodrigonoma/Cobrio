import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { HistoricoNotificacaoResponse, StatusNotificacao } from '../models/historico-notificacao.model';

@Injectable({
  providedIn: 'root'
})
export class NotificacoesService {
  private readonly apiUrl = `${environment.apiUrl}/Notificacoes`;

  constructor(private http: HttpClient) {}

  /**
   * Lista notificações com filtros opcionais
   */
  listar(filtros?: {
    dataInicio?: Date;
    dataFim?: Date;
    status?: StatusNotificacao;
    emailDestinatario?: string;
  }): Observable<HistoricoNotificacaoResponse[]> {
    let params = new HttpParams();

    if (filtros?.dataInicio) {
      params = params.set('dataInicio', filtros.dataInicio.toISOString());
    }

    if (filtros?.dataFim) {
      params = params.set('dataFim', filtros.dataFim.toISOString());
    }

    if (filtros?.status !== undefined) {
      params = params.set('status', filtros.status.toString());
    }

    if (filtros?.emailDestinatario) {
      params = params.set('emailDestinatario', filtros.emailDestinatario);
    }

    return this.http.get<HistoricoNotificacaoResponse[]>(this.apiUrl, { params });
  }

  /**
   * Obtém detalhes de uma notificação específica
   */
  obterPorId(id: string): Observable<HistoricoNotificacaoResponse> {
    return this.http.get<HistoricoNotificacaoResponse>(`${this.apiUrl}/${id}`);
  }
}

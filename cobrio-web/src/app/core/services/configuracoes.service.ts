import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EmailConfig {
  emailRemetente?: string;
  nomeRemetente?: string;
  emailReplyTo?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ConfiguracoesService {
  private readonly apiUrl = `${environment.apiUrl}/configuracoes`;

  constructor(private http: HttpClient) {}

  getEmailConfig(): Observable<EmailConfig> {
    return this.http.get<EmailConfig>(`${this.apiUrl}/email`);
  }

  updateEmailConfig(config: EmailConfig): Observable<EmailConfig> {
    return this.http.put<EmailConfig>(`${this.apiUrl}/email`, config);
  }
}

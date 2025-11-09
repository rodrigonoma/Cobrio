import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import {
  TemplateEmail,
  CreateTemplateEmailRequest,
  UpdateTemplateEmailRequest
} from '../models/template-email.models';

@Injectable({
  providedIn: 'root'
})
export class TemplateEmailService {

  constructor(private api: ApiService) { }

  getAll(): Observable<TemplateEmail[]> {
    return this.api.get<TemplateEmail[]>('TemplatesEmail');
  }

  getById(id: string): Observable<TemplateEmail> {
    return this.api.get<TemplateEmail>(`TemplatesEmail/${id}`);
  }

  create(request: CreateTemplateEmailRequest): Observable<TemplateEmail> {
    return this.api.post<TemplateEmail>('TemplatesEmail', request);
  }

  update(id: string, request: UpdateTemplateEmailRequest): Observable<TemplateEmail> {
    return this.api.put<TemplateEmail>(`TemplatesEmail/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`TemplatesEmail/${id}`);
  }
}

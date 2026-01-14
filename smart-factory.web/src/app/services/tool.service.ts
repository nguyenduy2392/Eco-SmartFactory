import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app.config';
import { Tool, CreateToolRequest, UpdateToolRequest } from '../models/tool.interface';

@Injectable({
  providedIn: 'root'
})
export class ToolService {
  private apiUrl = `${AppConfig.getApiUrl()}/tools`;

  constructor(private http: HttpClient) { }

  getAll(isActive?: boolean, status?: string): Observable<Tool[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<Tool[]>(this.apiUrl, { params });
  }

  getById(id: string): Observable<Tool> {
    return this.http.get<Tool>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateToolRequest): Observable<Tool> {
    return this.http.post<Tool>(this.apiUrl, request);
  }

  update(id: string, request: UpdateToolRequest): Observable<Tool> {
    return this.http.put<Tool>(`${this.apiUrl}/${id}`, request);
  }
}


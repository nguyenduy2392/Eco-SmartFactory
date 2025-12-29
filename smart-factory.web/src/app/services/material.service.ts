import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Material, CreateMaterialRequest, UpdateMaterialRequest } from '../models/material.interface';

@Injectable({
  providedIn: 'root'
})
export class MaterialService {
  private apiUrl = `${environment.apiUrl}/materials`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả materials (có thể filter theo customer)
   */
  getAll(isActive?: boolean, customerId?: string): Observable<Material[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    if (customerId) {
      params = params.set('customerId', customerId);
    }
    return this.http.get<Material[]>(this.apiUrl, { params });
  }

  /**
   * Lấy danh sách materials theo chủ hàng
   */
  getByCustomer(customerId: string, isActive?: boolean): Observable<Material[]> {
    return this.getAll(isActive, customerId);
  }

  getById(id: string): Observable<Material> {
    return this.http.get<Material>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateMaterialRequest): Observable<Material> {
    return this.http.post<Material>(this.apiUrl, request);
  }

  update(id: string, request: UpdateMaterialRequest): Observable<Material> {
    return this.http.put<Material>(`${this.apiUrl}/${id}`, request);
  }
}


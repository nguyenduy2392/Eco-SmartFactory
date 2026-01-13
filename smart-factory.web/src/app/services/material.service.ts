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
   * @param isActive - Filter theo trạng thái active
   * @param customerId - Filter theo chủ hàng
   * @param excludeShared - Nếu true, chỉ lấy materials thuộc customerId, không bao gồm materials dùng chung
   */
  getAll(isActive?: boolean, customerId?: string, excludeShared?: boolean): Observable<Material[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    if (customerId) {
      params = params.set('customerId', customerId);
    }
    if (excludeShared !== undefined) {
      params = params.set('excludeShared', excludeShared.toString());
    }
    return this.http.get<Material[]>(this.apiUrl, { params });
  }

  /**
   * Lấy danh sách materials theo chủ hàng (bao gồm cả materials dùng chung)
   */
  getByCustomer(customerId: string, isActive?: boolean): Observable<Material[]> {
    return this.getAll(isActive, customerId, false);
  }

  /**
   * Lấy danh sách materials thuộc riêng chủ hàng (không bao gồm materials dùng chung)
   */
  getByCustomerOnly(customerId: string, isActive?: boolean): Observable<Material[]> {
    return this.getAll(isActive, customerId, true);
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


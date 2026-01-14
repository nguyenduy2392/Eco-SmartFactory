import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app.config';
import { MaterialReceipt, CreateMaterialReceiptRequest, UpdateMaterialReceiptRequest } from '../models/material-receipt.interface';

@Injectable({
  providedIn: 'root'
})
export class MaterialReceiptService {
  private apiUrl = `${AppConfig.getApiUrl()}/material-receipts`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách phiếu nhập kho (có thể filter theo customer hoặc material)
   */
  getAll(customerId?: string, materialId?: string, status?: string): Observable<MaterialReceipt[]> {
    let params = new HttpParams();
    if (customerId) {
      params = params.set('customerId', customerId);
    }
    if (materialId) {
      params = params.set('materialId', materialId);
    }
    if (status) {
      params = params.set('status', status);
    }
    return this.http.get<MaterialReceipt[]>(this.apiUrl, { params });
  }

  /**
   * Lấy danh sách phiếu nhập kho theo chủ hàng
   */
  getByCustomer(customerId: string, status?: string): Observable<MaterialReceipt[]> {
    return this.getAll(customerId, undefined, status);
  }

  /**
   * Lấy danh sách phiếu nhập kho theo nguyên vật liệu
   */
  getByMaterial(materialId: string, status?: string): Observable<MaterialReceipt[]> {
    return this.getAll(undefined, materialId, status);
  }

  /**
   * Lấy chi tiết phiếu nhập kho
   */
  getById(id: string): Observable<MaterialReceipt> {
    return this.http.get<MaterialReceipt>(`${this.apiUrl}/${id}`);
  }

  /**
   * Tạo phiếu nhập kho mới
   */
  create(request: CreateMaterialReceiptRequest): Observable<MaterialReceipt> {
    return this.http.post<MaterialReceipt>(this.apiUrl, request);
  }

  /**
   * Cập nhật phiếu nhập kho
   */
  update(id: string, request: UpdateMaterialReceiptRequest): Observable<MaterialReceipt> {
    return this.http.put<MaterialReceipt>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Xóa phiếu nhập kho
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Xác nhận nhập kho (chuyển status từ PENDING sang RECEIVED)
   */
  confirmReceipt(id: string): Observable<MaterialReceipt> {
    return this.http.post<MaterialReceipt>(`${this.apiUrl}/${id}/confirm`, {});
  }
}


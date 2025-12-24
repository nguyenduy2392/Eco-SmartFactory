import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PurchaseOrder,
  PurchaseOrderList,
  CreatePurchaseOrderRequest,
  UpdatePurchaseOrderRequest,
  ClonePOVersionRequest
} from '../models/purchase-order.interface';

@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {
  private apiUrl = `${environment.apiUrl}/purchaseorders`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả PO
   */
  getAll(status?: string, versionType?: string, customerId?: string): Observable<PurchaseOrderList[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (versionType) params = params.set('versionType', versionType);
    if (customerId) params = params.set('customerId', customerId);

    return this.http.get<PurchaseOrderList[]>(this.apiUrl, { params });
  }

  /**
   * Lấy chi tiết PO theo ID
   */
  getById(id: string): Observable<PurchaseOrder> {
    return this.http.get<PurchaseOrder>(`${this.apiUrl}/${id}`);
  }

  /**
   * Tạo PO mới
   */
  create(request: CreatePurchaseOrderRequest): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(this.apiUrl, request);
  }

  /**
   * Cập nhật PO
   */
  update(id: string, request: UpdatePurchaseOrderRequest): Observable<PurchaseOrder> {
    return this.http.put<PurchaseOrder>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Clone PO version (ORIGINAL -> FINAL -> PRODUCTION)
   */
  cloneVersion(request: ClonePOVersionRequest): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/clone-version`, request);
  }

  /**
   * Import PO từ Excel
   */
  importFromExcel(
    file: File,
    poNumber: string,
    customerId: string,
    templateType: string,
    poDate: Date,
    expectedDeliveryDate: Date | null,
    notes: string
  ): Observable<PurchaseOrder> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('poNumber', poNumber);
    formData.append('customerId', customerId);
    formData.append('templateType', templateType);
    formData.append('poDate', poDate.toISOString());
    if (expectedDeliveryDate) {
      formData.append('expectedDeliveryDate', expectedDeliveryDate.toISOString());
    }
    if (notes) {
      formData.append('notes', notes);
    }
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/import-excel`, formData);
  }
}


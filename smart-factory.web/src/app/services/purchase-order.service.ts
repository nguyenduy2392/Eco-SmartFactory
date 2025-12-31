import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  PurchaseOrder,
  PurchaseOrderList,
  ImportPORequest,
  ImportPOResponse,
  ClonePOVersionRequest,
  ApprovePORequest,
  AvailabilityCheckRequest,
  AvailabilityCheckResult
} from '../models/purchase-order.interface';

@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {
  private apiUrl = `${environment.apiUrl}/purchaseorders`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách Processing PO
   * Filter by: status, processingType, customerId
   */
  getAll(status?: string, processingType?: string, customerId?: string): Observable<PurchaseOrderList[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (processingType) params = params.set('processingType', processingType);
    if (customerId) params = params.set('customerId', customerId);

    return this.http.get<PurchaseOrderList[]>(this.apiUrl, { params });
  }

  /**
   * Lấy chi tiết PO theo ID
   * Returns APPROVED version if exists, otherwise latest DRAFT
   */
  getById(id: string): Observable<PurchaseOrder> {
    return this.http.get<PurchaseOrder>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy tất cả versions của một PO
   */
  getVersions(poNumber: string): Observable<PurchaseOrder[]> {
    return this.http.get<PurchaseOrder[]>(`${this.apiUrl}/versions/${poNumber}`);
  }

  /**
   * Import Processing PO từ Excel - ONLY way to create PO
   * Excel must contain exactly 2 sheets:
   * 1. NHAP_PO (PO Operations - pricing)
   * 2. NHAP_NGUYEN_VAT_LIEU (Material Receipt - nhập kho thực tế)
   * Returns: PO with version = V0, status = DRAFT
   * Material Receipts sẽ được tạo tự động và cập nhật tồn kho
   */
  importFromExcel(
    file: File,
    poNumber: string,
    customerId: string,
    processingType: 'EP_NHUA' | 'PHUN_IN' | 'LAP_RAP',
    poDate: Date,
    expectedDeliveryDate: Date | null,
    notes: string
  ): Observable<ImportPOResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('poNumber', poNumber);
    formData.append('customerId', customerId);
    formData.append('processingType', processingType);
    formData.append('poDate', poDate.toISOString());
    if (expectedDeliveryDate) {
      formData.append('expectedDeliveryDate', expectedDeliveryDate.toISOString());
    }
    if (notes) {
      formData.append('notes', notes);
    }
    return this.http.post<ImportPOResponse>(`${this.apiUrl}/import-excel`, formData);
  }

  /**
   * Clone PO để tạo version mới (V1, V2, ...)
   * New version status = DRAFT
   */
  cloneVersion(request: ClonePOVersionRequest): Observable<PurchaseOrder> {
    return this.http.post<PurchaseOrder>(`${this.apiUrl}/clone-version`, request);
  }

  /**
   * Approve PO version for PMC
   * - Set status = APPROVED_FOR_PMC
   * - Lock version (no further edits)
   * - Only ONE version can be APPROVED at a time
   */
  approveForPMC(request: ApprovePORequest): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/approve`, request);
  }

  /**
   * Xóa PO (only if status = DRAFT)
   */
  delete(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/${id}`);
  }

  /**
   * Export PO operations to Excel
   */
  exportOperations(purchaseOrderId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${purchaseOrderId}/export-operations`, {
      responseType: 'blob'
    });
  }

  /**
   * Update PO general information
   */
  updateGeneralInfo(purchaseOrderId: string, data: {
    poNumber?: string;
    customerId?: string;
    processingType?: string;
    poDate?: Date;
    expectedDeliveryDate?: Date;
    notes?: string;
  }): Observable<PurchaseOrder> {
    return this.http.put<PurchaseOrder>(`${this.apiUrl}/${purchaseOrderId}/general-info`, data);
  }

  /**
   * Create PO Operation
   */
  createOperation(purchaseOrderId: string, operation: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/${purchaseOrderId}/operations`, operation);
  }

  /**
   * Update PO Operation
   */
  updateOperation(purchaseOrderId: string, operationId: string, operation: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/${purchaseOrderId}/operations/${operationId}`, operation);
  }

  /**
   * Delete PO Operation
   */
  deleteOperation(purchaseOrderId: string, operationId: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/${purchaseOrderId}/operations/${operationId}`);
  }

  /**
   * Update PO Product (quantity, unit price)
   */
  updateProduct(purchaseOrderId: string, productId: string, data: {
    quantity: number;
    unitPrice?: number;
  }): Observable<any> {
    return this.http.put(`${this.apiUrl}/${purchaseOrderId}/products/${productId}`, data);
  }
}


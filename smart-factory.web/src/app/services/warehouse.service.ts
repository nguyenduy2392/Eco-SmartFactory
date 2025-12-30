import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Warehouse,
  MaterialReceipt,
  MaterialIssue,
  MaterialAdjustment,
  MaterialTransactionHistory,
  MaterialStock,
  CreateMaterialReceiptRequest,
  CreateMaterialIssueRequest,
  CreateMaterialAdjustmentRequest
} from '../models/warehouse.interface';

@Injectable({
  providedIn: 'root'
})
export class WarehouseService {
  private apiUrl = `${environment.apiUrl}/warehouse`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả kho
   */
  getAllWarehouses(isActive?: boolean): Observable<Warehouse[]> {
    let params = new HttpParams();
    if (isActive !== undefined) {
      params = params.set('isActive', isActive.toString());
    }
    return this.http.get<Warehouse[]>(this.apiUrl, { params });
  }

  /**
   * Nhập kho nguyên vật liệu
   */
  createMaterialReceipt(request: CreateMaterialReceiptRequest): Observable<MaterialReceipt> {
    return this.http.post<MaterialReceipt>(`${this.apiUrl}/receipt`, request);
  }

  /**
   * Xuất kho nguyên vật liệu
   */
  createMaterialIssue(request: CreateMaterialIssueRequest): Observable<MaterialIssue> {
    return this.http.post<MaterialIssue>(`${this.apiUrl}/issue`, request);
  }

  /**
   * Điều chỉnh kho nguyên vật liệu
   */
  createMaterialAdjustment(request: CreateMaterialAdjustmentRequest): Observable<MaterialAdjustment> {
    return this.http.post<MaterialAdjustment>(`${this.apiUrl}/adjustment`, request);
  }

  /**
   * Lấy lịch sử giao dịch kho
   */
  getTransactionHistory(params: {
    materialId?: string;
    customerId?: string;
    warehouseId?: string;
    batchNumber?: string;
    transactionType?: string;
    fromDate?: Date;
    toDate?: Date;
    pageNumber?: number;
    pageSize?: number;
  }): Observable<MaterialTransactionHistory[]> {
    let httpParams = new HttpParams();
    
    if (params.materialId) {
      httpParams = httpParams.set('materialId', params.materialId);
    }
    if (params.customerId) {
      httpParams = httpParams.set('customerId', params.customerId);
    }
    if (params.warehouseId) {
      httpParams = httpParams.set('warehouseId', params.warehouseId);
    }
    if (params.batchNumber) {
      httpParams = httpParams.set('batchNumber', params.batchNumber);
    }
    if (params.transactionType) {
      httpParams = httpParams.set('transactionType', params.transactionType);
    }
    if (params.fromDate) {
      httpParams = httpParams.set('fromDate', params.fromDate.toISOString());
    }
    if (params.toDate) {
      httpParams = httpParams.set('toDate', params.toDate.toISOString());
    }
    if (params.pageNumber) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params.pageSize) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<MaterialTransactionHistory[]>(`${this.apiUrl}/history`, { params: httpParams });
  }

  /**
   * Lấy thông tin tồn kho của nguyên vật liệu
   */
  getMaterialStock(materialId: string, warehouseId?: string): Observable<MaterialStock> {
    let params = new HttpParams();
    if (warehouseId) {
      params = params.set('warehouseId', warehouseId);
    }
    return this.http.get<MaterialStock>(`${this.apiUrl}/stock/${materialId}`, { params });
  }

  /**
   * Import nhập kho từ Excel
   */
  importMaterialReceipts(file: File, customerId: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('customerId', customerId);
    return this.http.post(`${this.apiUrl}/import-receipts`, formData);
  }

  /**
   * Export lịch sử giao dịch ra Excel
   */
  exportTransactionHistory(params: {
    materialId?: string;
    customerId?: string;
    warehouseId?: string;
    batchNumber?: string;
    transactionType?: string;
    fromDate?: Date;
    toDate?: Date;
  }): Observable<Blob> {
    let httpParams = new HttpParams();
    
    if (params.materialId) {
      httpParams = httpParams.set('materialId', params.materialId);
    }
    if (params.customerId) {
      httpParams = httpParams.set('customerId', params.customerId);
    }
    if (params.warehouseId) {
      httpParams = httpParams.set('warehouseId', params.warehouseId);
    }
    if (params.batchNumber) {
      httpParams = httpParams.set('batchNumber', params.batchNumber);
    }
    if (params.transactionType) {
      httpParams = httpParams.set('transactionType', params.transactionType);
    }
    if (params.fromDate) {
      httpParams = httpParams.set('fromDate', params.fromDate.toISOString());
    }
    if (params.toDate) {
      httpParams = httpParams.set('toDate', params.toDate.toISOString());
    }

    return this.http.get(`${this.apiUrl}/export-history`, { 
      params: httpParams,
      responseType: 'blob'
    });
  }
}


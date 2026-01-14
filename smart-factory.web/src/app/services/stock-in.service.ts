import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app.config';
import {
  StockInRequest,
  StockInResponse,
  MaterialReceiptHistory,
  POForSelection
} from '../models/stock-in.interface';

@Injectable({
  providedIn: 'root'
})
export class StockInService {
  private apiUrl = `${AppConfig.getApiUrl()}/stockin`;
  private poApiUrl = `${AppConfig.getApiUrl()}/purchaseorders`;

  constructor(private http: HttpClient) { }

  /**
   * Nhập kho nguyên vật liệu
   * Có thể gắn hoặc không gắn PO
   */
  stockIn(request: StockInRequest): Observable<StockInResponse> {
    return this.http.post<StockInResponse>(this.apiUrl, request);
  }

  /**
   * Lấy lịch sử nhập kho của một PO
   */
  getPOReceiptHistory(purchaseOrderId: string): Observable<MaterialReceiptHistory[]> {
    return this.http.get<MaterialReceiptHistory[]>(`${this.poApiUrl}/${purchaseOrderId}/receipt-history`);
  }

  /**
   * Lấy danh sách PO cho dropdown/search khi nhập kho
   * Filter theo customerId nếu được cung cấp
   */
  getPOsForSelection(searchTerm?: string, customerId?: string): Observable<POForSelection[]> {
    let params = new HttpParams();
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    if (customerId) {
      params = params.set('customerId', customerId);
    }
    return this.http.get<POForSelection[]>(`${this.poApiUrl}/for-selection`, { params });
  }

  /**
   * Lấy thông tin PO theo ID để pre-fill form
   */
  getPOById(poId: string): Observable<POForSelection> {
    return this.http.get<POForSelection>(`${this.poApiUrl}/${poId}/for-selection`);
  }
}

import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ProcessBOM,
  ProcessBOMList,
  CreateBOMRequest,
  GetBOMHistoryRequest
} from '../models/process-bom.interface';

@Injectable({
  providedIn: 'root'
})
export class ProcessBOMService {
  private apiUrl = `${environment.apiUrl}/processbom`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách Process BOM
   * Filter by: partId, processingType, status
   */
  getAll(partId?: string, processingType?: string, status?: string): Observable<ProcessBOMList[]> {
    let params = new HttpParams();
    if (partId) params = params.set('partId', partId);
    if (processingType) params = params.set('processingType', processingType);
    if (status) params = params.set('status', status);

    return this.http.get<ProcessBOMList[]>(this.apiUrl, { params });
  }

  /**
   * Lấy chi tiết BOM theo ID
   */
  getById(id: string): Observable<ProcessBOM> {
    return this.http.get<ProcessBOM>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy ACTIVE BOM cho một part + processing type
   */
  getActiveBOM(partId: string, processingType: string): Observable<ProcessBOM | null> {
    return this.http.get<ProcessBOM | null>(`${this.apiUrl}/active`, {
      params: new HttpParams()
        .set('partId', partId)
        .set('processingType', processingType)
    });
  }

  /**
   * Lấy BOM history cho một part + processing type
   */
  getBOMHistory(request: GetBOMHistoryRequest): Observable<ProcessBOM[]> {
    return this.http.get<ProcessBOM[]>(`${this.apiUrl}/history`, {
      params: new HttpParams()
        .set('partId', request.partId)
        .set('processingType', request.processingType)
    });
  }

  /**
   * Tạo BOM version mới
   * - Automatically sets old ACTIVE BOM to INACTIVE
   * - New BOM must have at least one material line
   */
  create(request: CreateBOMRequest): Observable<ProcessBOM> {
    return this.http.post<ProcessBOM>(this.apiUrl, request);
  }

  /**
   * Xóa BOM (only if not ACTIVE)
   */
  delete(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.delete<{ success: boolean; message: string }>(`${this.apiUrl}/${id}`);
  }
}



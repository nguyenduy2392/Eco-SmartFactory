import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PartDetail {
  id: string;
  code: string;
  name: string;
  productId: string;
  productName?: string;
  productCode?: string;
  position?: string;
  material?: string;
  color?: string;
  weight?: number;
  description?: string;
  isActive: boolean;
  createdAt: Date;
  status: string;
  processes: ProcessType[];
}

export interface ProcessType {
  id: string;
  name: string;
  code: string;
  description?: string;
  icon: string;
  color: string;
  stages: ProductionOperation[];
}

export interface ProductionOperation {
  id: string;
  operationName: string;
  machineId?: string;
  machineName?: string;
  cycleTime?: number;
  sequenceOrder: number;
  status: string;
  materials: OperationMaterial[];
  tools: OperationTool[];
}

export interface OperationMaterial {
  id: string;
  name: string;
  code: string;
  quantity: number;
  unit: string;
}

export interface OperationTool {
  id: string;
  name: string;
  toolId: string;
  code?: string;
}

@Injectable({
  providedIn: 'root'
})
export class PartService {
  private apiUrl = `${environment.apiUrl}/parts`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy chi tiết linh kiện theo ID và PO ID
   */
  getById(partId: string, purchaseOrderId: string): Observable<PartDetail> {
    const params = new HttpParams().set('purchaseOrderId', purchaseOrderId);
    return this.http.get<PartDetail>(`${this.apiUrl}/${partId}`, { params });
  }
}


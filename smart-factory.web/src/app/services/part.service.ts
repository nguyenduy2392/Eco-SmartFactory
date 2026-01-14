import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app.config';
import { PartProcessingType, CreatePartProcessingTypeRequest, UpdatePartProcessingTypeRequest } from '../models/processing-type.interface';

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
  processingTypes?: PartProcessingType[]; // Các loại hình gia công mà linh kiện này có thể trải qua
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
  private apiUrl = `${AppConfig.getApiUrl()}/parts`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả parts
   */
  getAll(): Observable<PartDetail[]> {
    return this.http.get<PartDetail[]>(this.apiUrl);
  }

  /**
   * Lấy chi tiết linh kiện theo ID và PO ID
   */
  getById(partId: string, purchaseOrderId: string): Observable<PartDetail> {
    const params = new HttpParams().set('purchaseOrderId', purchaseOrderId);
    return this.http.get<PartDetail>(`${this.apiUrl}/${partId}`, { params });
  }

  /**
   * Lấy danh sách loại hình gia công của một linh kiện
   */
  getProcessingTypes(partId: string): Observable<PartProcessingType[]> {
    return this.http.get<PartProcessingType[]>(`${this.apiUrl}/${partId}/processing-types`);
  }

  /**
   * Thêm loại hình gia công cho linh kiện
   */
  addProcessingType(partId: string, request: CreatePartProcessingTypeRequest): Observable<PartProcessingType> {
    return this.http.post<PartProcessingType>(`${this.apiUrl}/${partId}/processing-types`, request);
  }

  /**
   * Cập nhật quan hệ PartProcessingType
   */
  updateProcessingType(partId: string, partProcessingTypeId: string, request: UpdatePartProcessingTypeRequest): Observable<PartProcessingType> {
    return this.http.put<PartProcessingType>(`${this.apiUrl}/${partId}/processing-types/${partProcessingTypeId}`, request);
  }

  /**
   * Xóa loại hình gia công khỏi linh kiện
   */
  removeProcessingType(partId: string, partProcessingTypeId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${partId}/processing-types/${partProcessingTypeId}`);
  }
}


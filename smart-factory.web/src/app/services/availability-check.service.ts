import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AvailabilityCheckRequest,
  AvailabilityCheckResult
} from '../models/purchase-order.interface';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityCheckService {
  private apiUrl = `${environment.apiUrl}/availabilitycheck`;

  constructor(private http: HttpClient) { }

  /**
   * Kiểm tra khả dụng nguyên vật liệu
   * 
   * Input:
   * - PO ID (must be APPROVED version)
   * - Planned production quantity
   * 
   * Calculation:
   * For each material:
   * Required_Qty = Planned_Qty × BOM_Qty × (1 + Scrap_Rate)
   * Available_Qty = Inventory_Qty + PO_Material_Baseline_Qty
   * Shortage = Required_Qty - Available_Qty
   * 
   * Result:
   * - Shortage > 0 → FAIL (CRITICAL)
   * - Available_Qty < Required_Qty × 1.1 → WARNING
   * - Else → PASS
   * 
   * IMPORTANT:
   * - Does NOT change inventory
   * - Does NOT create production data
   * - Does NOT affect pricing
   */
  checkAvailability(request: AvailabilityCheckRequest): Observable<AvailabilityCheckResult> {
    return this.http.post<AvailabilityCheckResult>(`${this.apiUrl}/check`, request);
  }
}



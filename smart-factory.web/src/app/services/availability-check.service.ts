import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../config/app.config';
import {
  AvailabilityCheckRequest,
  AvailabilityCheckResult
} from '../models/purchase-order.interface';

@Injectable({
  providedIn: 'root'
})
export class AvailabilityCheckService {
  private apiUrl = `${AppConfig.getApiUrl()}/availabilitycheck`;

  constructor(private http: HttpClient) { }

  /**
   * Kiểm tra khả dụng linh kiện theo PO
   * 
   * Input:
   * - PO ID (must be APPROVED version)
   * - Planned production quantity
   * 
   * Calculation:
   * For each part in PO Operations:
   * Required_Qty = Planned_Qty × PO_Operation_Quantity
   * CanProduce = Has ACTIVE BOM for (Part + ProcessingType)
   * 
   * Result:
   * - No ACTIVE BOM → FAIL (CRITICAL)
   * - Has ACTIVE BOM → PASS
   * 
   * IMPORTANT:
   * - Does NOT change inventory
   * - Does NOT create production data
   * - Does NOT affect pricing
   */
  checkAvailability(request: AvailabilityCheckRequest): Observable<AvailabilityCheckResult> {
    return this.http.post<AvailabilityCheckResult>(`${this.apiUrl}/check`, request);
  }

  /**
   * Kiểm tra khả dụng linh kiện theo component (không cần PO)
   * 
   * Input:
   * - Part ID
   * - Processing Type ID
   * - Quantity
   * 
   * Calculation:
   * CanProduce = Has ACTIVE BOM for (Part + ProcessingType)
   * 
   * Result:
   * - No ACTIVE BOM → FAIL (CRITICAL)
   * - Has ACTIVE BOM → PASS
   */
  checkAvailabilityByComponent(request: AvailabilityCheckRequest): Observable<AvailabilityCheckResult> {
    return this.http.post<AvailabilityCheckResult>(`${this.apiUrl}/check-by-component`, request);
  }
}



import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface PMCWeekDto {
  id: string;
  weekStartDate: string;
  weekEndDate: string;
  version: number;
  weekName: string;
  isActive: boolean;
  status: string;
  notes?: string;
  createdBy: string;
  createdByName?: string;
  createdAt: string;
  updatedAt?: string;
  weekDates: string[];
  rows: PMCRowDto[];
}

export interface PMCRowDto {
  id: string;
  pmcWeekId: string;
  productCode: string;
  componentName: string;
  customerId?: string;
  customerName?: string;
  planType: string;
  planTypeDisplay: string;
  displayOrder: number;
  totalValue?: number;
  rowGroup: string;
  notes?: string;
  cells: PMCCellDto[];
}

export interface PMCCellDto {
  id: string;
  pmcRowId: string;
  workDate: string;
  value: number;
  isEditable: boolean;
  backgroundColor?: string;
  notes?: string;
}

export interface PMCWeekListItemDto {
  id: string;
  weekStartDate: string;
  weekEndDate: string;
  version: number;
  weekName: string;
  isActive: boolean;
  status: string;
  createdByName?: string;
  createdAt: string;
  totalRows: number;
}

export interface CreatePMCWeekRequest {
  weekStartDate?: string;
  notes?: string;
  copyFromPreviousWeek?: boolean;
}

export interface SavePMCWeekRequest {
  pmcWeekId: string;
  notes?: string;
  rows: SavePMCRowRequest[];
}

export interface SavePMCRowRequest {
  id?: string;
  productCode: string;
  componentName: string;
  customerId?: string;
  planType: string;
  totalValue?: number;
  notes?: string;
  cellValues: { [key: string]: number };
}

@Injectable({
  providedIn: 'root'
})
export class PMCService {
  private apiUrl = `${environment.apiUrl}/pmc`;

  constructor(private http: HttpClient) { }

  /**
   * Get PMC week by ID or by date (no versioning - one PMC per week)
   */
  getPMCWeek(id?: string, weekStart?: string): Observable<PMCWeekDto> {
    let params = new HttpParams();
    
    if (id) {
      params = params.set('id', id);
    } else if (weekStart) {
      params = params.set('weekStart', weekStart);
    }
    
    return this.http.get<PMCWeekDto>(this.apiUrl, { params });
  }

  /**
   * Get list of PMC weeks
   */
  getPMCWeeks(fromDate?: string, toDate?: string, onlyActive: boolean = false, take?: number): Observable<PMCWeekListItemDto[]> {
    let params = new HttpParams();
    
    if (fromDate) {
      params = params.set('fromDate', fromDate);
    }
    if (toDate) {
      params = params.set('toDate', toDate);
    }
    params = params.set('onlyActive', onlyActive.toString());
    if (take) {
      params = params.set('take', take.toString());
    }
    
    return this.http.get<PMCWeekListItemDto[]>(`${this.apiUrl}/list`, { params });
  }

  /**
   * Get previous week's PMC
   */
  getPreviousPMCWeek(weekStart: string): Observable<PMCWeekDto> {
    const params = new HttpParams().set('weekStart', weekStart);
    return this.http.get<PMCWeekDto>(`${this.apiUrl}/previous`, { params });
  }

  /**
   * Create a new PMC week
   */
  createPMCWeek(request: CreatePMCWeekRequest): Observable<PMCWeekDto> {
    return this.http.post<PMCWeekDto>(`${this.apiUrl}/create`, request);
  }

  /**
   * Save PMC week (updates existing data, no versioning)
   */
  savePMCWeek(request: SavePMCWeekRequest): Observable<PMCWeekDto> {
    return this.http.post<PMCWeekDto>(`${this.apiUrl}/save`, request);
  }

  /**
   * Debug: Get available POs for PMC creation
   */
  getAvailablePOs(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/debug/available-pos`);
  }
}

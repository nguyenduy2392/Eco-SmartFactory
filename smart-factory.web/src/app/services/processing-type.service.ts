import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ProcessingType, ProcessMethod } from '../models/processing-type.interface';

@Injectable({
  providedIn: 'root'
})
export class ProcessingTypeService {
  private apiUrl = `${environment.apiUrl}/processing-types`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả loại hình gia công
   */
  getAll(): Observable<ProcessingType[]> {
    return this.http.get<ProcessingType[]>(this.apiUrl);
  }

  /**
   * Lấy chi tiết loại hình gia công theo ID
   */
  getById(id: string): Observable<ProcessingType> {
    return this.http.get<ProcessingType>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy danh sách công đoạn gia công (ProcessMethod) của một loại hình gia công
   */
  getProcessMethods(processingTypeId: string): Observable<ProcessMethod[]> {
    return this.http.get<ProcessMethod[]>(`${this.apiUrl}/${processingTypeId}/process-methods`);
  }
}



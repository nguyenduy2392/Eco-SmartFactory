import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UnitOfMeasure, CreateUnitOfMeasureRequest } from '../models/unit-of-measure.interface';

@Injectable({
  providedIn: 'root'
})
export class UnitOfMeasureService {
  private apiUrl = `${environment.apiUrl}/UnitsOfMeasure`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<UnitOfMeasure[]> {
    return this.http.get<UnitOfMeasure[]>(this.apiUrl);
  }

  getById(id: string): Observable<UnitOfMeasure> {
    return this.http.get<UnitOfMeasure>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateUnitOfMeasureRequest): Observable<UnitOfMeasure> {
    return this.http.post<UnitOfMeasure>(this.apiUrl, request);
  }

  update(id: string, request: CreateUnitOfMeasureRequest): Observable<UnitOfMeasure> {
    return this.http.put<UnitOfMeasure>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

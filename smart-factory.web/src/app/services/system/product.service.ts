import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfig } from '../../config/app.config';
import { Product, ProductWithPrice, CreateProductRequest, UpdateProductRequest } from '../../models/interface/product.interface';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = `${AppConfig.getApiUrl()}/products`;

  constructor(private http: HttpClient) { }

  /**
   * Lấy danh sách tất cả sản phẩm
   */
  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  /**
   * Lấy danh sách sản phẩm với giá
   */
  getAllWithPrices(): Observable<ProductWithPrice[]> {
    return this.http.get<ProductWithPrice[]>(`${this.apiUrl}?withPrices=true`);
  }

  /**
   * Lấy chi tiết sản phẩm theo ID
   */
  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  /**
   * Lấy chi tiết sản phẩm trong PO (bao gồm danh sách linh kiện)
   */
  getDetailByPO(productId: string, purchaseOrderId: string): Observable<any> {
    const params = new HttpParams().set('purchaseOrderId', purchaseOrderId);
    return this.http.get<any>(`${this.apiUrl}/${productId}/detail`, { params });
  }

  /**
   * Tạo sản phẩm mới
   */
  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, request);
  }

  /**
   * Cập nhật sản phẩm
   */
  update(id: string, request: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Xóa sản phẩm
   */
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}

import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /** Thêm mới hàng hóa
   * @param model 
   * @returns kết quả thêm mới và id của entity vừa thêm
   */
  CreateProduct(model: any) {
    return this.master.post(`${this.apiURL}api/product`, model);
  }

  /**
   * Cập nhật thông tin hàng hóa
   * @param model Thông tin hàng hóa
   * @returns Kế quả cập nhật hàng hóa
   */
  UpdateProduct(model: any) {
    return this.master.put(`${this.apiURL}api/product`, model);
  }

  /** Nhân bản hàng hóa
   * @param id Id hàng hóa cần nhân bản
   * @returns 
   */
  DuplicateProduct(id: any) {
    return this.master.post(`${this.apiURL}api/product/${id}`, null);
  }

  /** Danh sách hàng hóa phân trang
   * @param filter 
   * @returns Danh sách hàng hóa phân trang
   */
  GetProductPaging(filter: any) {
    let param = ToUrlParam(filter);
    return this.master.get(`${this.apiURL}api/product/paging${param}`);
  }

  /** Chi tiết hàng hóa theo id
   * @param id Id hàng hóa
   * @returns 
   */
  GetProductById(id: any) {
    return this.master.get(`${this.apiURL}api/product/${id}`);
  }

  /** Xóa vật tư hàng hóa
   * @param id hàng hóa cần xóa
   * @returns 
   */
  DeleteProductById(id: any) {
    return this.master.delete(`${this.apiURL}api/product/${id}`);
  }

  /**
   * Nhập khẩu dữ liệu từ tệp excel
   * @param form Dữ liệu form data chứa file
   * @returns 
   */
  ImportProduct(form: FormData) {
    return this.master.post(`${this.apiURL}api/product/import`, form);
  }

  /** Xuất khẩu kết quả nhập khẩu
   * @param model Xuất khẩu kết quả nhập khẩu
   * @returns 
   */
  ExportImportResult(model: any) {
    return this.master.post(`${this.apiURL}api/product/export-import-result`, model);
  }

  /** Xuất khẩu danh sách vật tư hàng hóa
   * @param filter 
   * @returns 
   */
  Export(filter: any) {
    let param = ToUrlParam(filter);
    return this.master.get(`${this.apiURL}api/product/export${param}`);
  }

  /** Toàn bộ vật tư hàng hóa
   * @returns 
   */
  GetAllProduct() {
    return this.master.get(`${this.apiURL}api/product/get-all`);
  }


  /** Tìm kiếm hàng hóa
   * @param keyword từ khóa tìm kiếm
   * @param quantity số lượng kết quả
   * @returns 
   */
  OnSearchProduct(keyword: any, quantity: number) {
    return this.master.get(`${this.apiURL}api/product/search?keyword=${keyword}&quantity=${quantity}`);
  }
}

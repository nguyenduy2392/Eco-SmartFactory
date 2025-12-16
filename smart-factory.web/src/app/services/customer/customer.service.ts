import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class CustomerService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
    private http: HttpClient,
  ) { }

  /** Lấy dữ liệu tài khoản công ty từ mã số thuế
   * @param taxCode Mã số thuế
   * @returns 
   */
  GetInfoByTaxCode(taxCode: string) {
    let url = `https://mst.pmbk.vn/find/${taxCode}`;

    return this.http.get<any>(url);
  }
  /** Thêm mới khách hàng
   * @param model 
   * @returns kết quả thêm mới và id của entity vừa thêm
   */
  CreateCustomer(model: any) {
    return this.master.post(`${this.apiURL}api/customer`, model);
  }

  /**
   * Cập nhật thông tin khách hàng
   * @param model Thông tin hàng hóa
   * @returns Kế quả cập nhật hàng hóa
   */
  UpdateCustomer(model: any) {
    return this.master.put(`${this.apiURL}api/customer`, model);
  }

  /** Danh sách khách hàng phân trang
   * @param filter Bộ lọc dữ liệu
   * @returns Danh sách khách hàng phân trang
   */
  GetCustomerPaging(filter: any) {
    let param = ToUrlParam(filter);
    return this.master.get(`${this.apiURL}api/customer${param}`);
  }

  /** Xóa khách hàng
   * @param id Định danh khách hàng
   * @returns Kết quả xóa IsSuccess
   */
  DeleteCustomer(id: string) {
    return this.master.delete(`${this.apiURL}api/customer/${id}`);
  }

  /** Nhân bản khách hàng
   * @param id Định danh khách hàng
   * @returns Thông tin khách hàng được nhân bản
   */
  DuplicateCustomer(id: string) {
    return this.master.post(`${this.apiURL}api/customer/${id}`, null);
  }

  /** Nhập khẩu dữ liệu từ tệp excel
   * @param form Dữ liệu form data chứa file
   * @returns Kết quả nhập khẩu
   */
  ImportCustomer(form: FormData) {
    return this.master.post(`${this.apiURL}api/customer/import`, form);
  }

  /** Xuất khẩu kết quả nhập khẩu
   * @param data kết quả nhập khẩu
   * @returns file base64 kết quả nhập khẩu
   */
  ExportImportResult(data: any) {
    return this.master.post(`${this.apiURL}api/customer/export-import-result`, data);
  }

  /** Xuất khẩu danh sách khách hàng
   * @param filter Bộ lọc dữ liệu
   * @returns 
   */
  ExportExcel(filter: any) {
    let param = ToUrlParam(filter);
    return this.master.get(`${this.apiURL}api/customer/export${param}`);
  }

  /** Toàn bộ khách hàng
   * @returns Danh sách khách hàng
   */
  GetAllCustomer() {
    return this.master.get(`${this.apiURL}api/customer/get-all`);
  }

  /** Sự kiện tìm kiếm khách hàng theo từ khóa
   * @param keyword từ khóa tìm kiếm
   * @param top số lượng kết quả
   * @returns Danh sách khách hàng
   */
  OnSearchCustomer(keyword?: string, top?: number) {
    return this.master.get(`${this.apiURL}api/customer/on-search?keyword=${keyword}&top=${top}`);
  }

  /** Chi tiết khách hàng theo id
   * @param id định danh khách hàng
   * @returns dữ liệu khách hàng
   */
  GetById(id: any) {
    return this.master.get(`${this.apiURL}api/customer/${id}`);
  }
}

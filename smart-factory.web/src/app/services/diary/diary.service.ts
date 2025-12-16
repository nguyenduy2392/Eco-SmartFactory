import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';

@Injectable({
  providedIn: 'root'
})
export class DiaryService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /** Chi tiết phiếu xuất kho theo id
   * @param id Định danh phiếu
   * @returns 
   */
  GetById(id: any) {
    return this.master.get(`${this.apiURL}api/diary/${id}`);
  }

  /** Mã phiếu nhập mới nhất
   * @returns Mã phiếu nhập
   */
  GetDiaryCode() {
    return this.master.get(`${this.apiURL}api/diary/lastest-code`);
  }

  /** Thêm mới phiếu xuất kho
   * @param model 
   * @returns 
   */
  CreateExportDiary(model: any) {
    return this.master.post(`${this.apiURL}api/diary`, model);
  }

  /** Cập nhật phiếu xuất kho
  * @param model 
  * @returns 
  */
  UpdateExportDiary(model: any) {
    return this.master.put(`${this.apiURL}api/diary`, model);
  }

  /** Danh sách phiếu xuất kho phân trang
   * @param model 
   * @returns 
   */
  GetDiaryPaging(model: any) {
    let param = ToUrlParam(model);
    return this.master.get(`${this.apiURL}api/diary/paging${param}`);
  }

  /** Xóa phiếu xuất kho
   * @param id Định danh phiếu xuất
   * @returns 
   */
  Delete(id: any) {
    return this.master.delete(`${this.apiURL}api/diary/${id}`);
  }

  /** Nhân bản phiếu xuất kho
   * @param id Định danh phiếu xuất
   * @returns 
   */
  Duplicate(id: any) {
    return this.master.post(`${this.apiURL}api/diary/duplicate/${id}`, null);

  }

  /**
   * Tạo tệp pdf cho phiếu kho
   * @param id Định danh phiếu xuất
   * @returns 
   */
  PrintPdf(id: any,isLanscape : boolean) {
    return this.master.get(`${this.apiURL}api/diary/print-pdf/${id}?&isLanscape=${isLanscape}`);
  }

  /**
   * Tạo tệp pdf cho phiếu kho
   * @param id Định danh phiếu xuất
   * @returns 
   */
  PrintExcel(id: any) {
    return this.master.get(`${this.apiURL}api/diary/print-excel/${id}`);
  }
}

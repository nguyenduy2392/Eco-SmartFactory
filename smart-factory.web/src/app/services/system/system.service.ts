import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';

@Injectable({
  providedIn: 'root'
})
export class SystemService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /// Thông tin cài đặt
  GetAppConfig() {
    return this.master.get(`${this.apiURL}api/system/get-config`);
  }

  /// Cập nhật thông tin cài đặt
  SetAppConfig(model: any) {
    return this.master.put(`${this.apiURL}api/system/set-config`, model);
  }

  /// Nhật ký hệ thống
  GetAuditLog(model: any) {
    let param = ToUrlParam(model);
    return this.master.get(`${this.apiURL}api/system/system-history${param}`);
  }

  /// Danh sách nhóm người dùng
  GetAllRole(keyword: any) {
    if (IsNull(keyword)) {
      return this.master.get(`${this.apiURL}api/system/roles`);
    }
    return this.master.get(`${this.apiURL}api/system/roles?keyword=${keyword}`);
  }

  /// Chi tiết người dùng
  GetRoleDetail(id?: any) {
    if (IsNull(id)) {
      return this.master.get(`${this.apiURL}api/system/role/get-by-id`);
    }
    return this.master.get(`${this.apiURL}api/system/role/get-by-id?id=${id}`);
  }

  /// Thêm mới hoặc cập nhật nhóm quyền
  CreateOrUpdate(model : any){
    return this.master.post(`${this.apiURL}api/system/role/create-or-update`,model);
  }
}

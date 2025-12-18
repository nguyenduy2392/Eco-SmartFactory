import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /// Thêm mới hoặc cập nhật người dùng
  CreateOrUpdateUser(model: any) {
    return this.master.post(`${this.apiURL}api/user/create-or-update`, model);
  }

  /// Danh sách người dùng
  GetAllUser(model?: any) {
    if (!IsNull(model)) {
      let param = ToUrlParam(model);
      return this.master.get(`${this.apiURL}api/user/get-all${param}`);
    } else {
      return this.master.get(`${this.apiURL}api/user/get-all`);
    }

  }

  /// Xóa người dùng
  DeleteUser(id: any) {
    return this.master.delete(`${this.apiURL}api/user/delete/${id}`);
  }

}

import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  public apiURL = this.env.baseApiUrl;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /// Lấy danh sách người dùng
  GetAllUsers(keyword?: string | null, isActive?: boolean | null) {
    let params = new URLSearchParams();
    if (keyword) params.append('keyword', keyword);
    if (isActive !== undefined && isActive !== null) {
      params.append('isActive', isActive.toString());
    }
    
    const queryString = params.toString();
    const url = queryString 
      ? `${this.apiURL}api/users?${queryString}` 
      : `${this.apiURL}api/users`;
    
    return this.master.get(url);
  }

  /// Lấy thông tin người dùng theo ID
  GetUserById(id: number) {
    return this.master.get(`${this.apiURL}api/users/${id}`);
  }

  /// Lấy thông tin người dùng hiện tại
  GetCurrentUser() {
    return this.master.get(`${this.apiURL}api/users/me`);
  }

  /// Tạo người dùng mới
  CreateUser(model: any) {
    return this.master.post(`${this.apiURL}api/users`, model);
  }

  /// Cập nhật người dùng
  UpdateUser(id: number, model: any) {
    return this.master.put(`${this.apiURL}api/users/${id}`, model);
  }

  /// Xóa người dùng
  DeleteUser(id: number) {
    return this.master.delete(`${this.apiURL}api/users/${id}`);
  }
}

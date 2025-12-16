import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MasterService } from './master/master.service';
import { EnvService } from '../env.service';

@Injectable({
  providedIn: 'root'
})
export class FileService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
    private http: HttpClient,
  ) { }

  /** Tải tệp dữ liệu lên hệ thống 
   * @param file Nội dung tệp
   * @param folder Thư mục lưu trữ trên server
   * @returns Kết quả tải lên là đường dẫn tương đối của tệp
   */
  UploadImage(file: any, folder: any) {
    return this.master.post(`${this.apiURL}api/file?folder=${folder}`, file);
  }
}

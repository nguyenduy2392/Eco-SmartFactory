import { Component, HostListener, OnInit } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { NgClass, NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import { JwtHelperService } from '@auth0/angular-jwt';
import { UiConfirm, UiModal } from '../../../../models/interface/uiInterface';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MenuItem } from 'primeng/api';
import moment from 'moment';
import { EnvService } from '../../../../env.service';
import { GenerateFile } from '../../../../shared/helper';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { IsNull } from '../../../../services/shared/common';
import { DataTypes, FontSizes, FontStyles, InspectionTypes, Symbols, Units } from '../../../../services/shared/default-data';
import { Router } from '@angular/router';
import { UiConfirmService } from '../../../../services/shared/ui-confirm.service';
import { TextGlobalConstants } from '../../../../shared/TextGlobalContants';
import { SystemService } from '../../../../services/system/system.service';
import { DomSanitizer } from '@angular/platform-browser';
import { FileService } from '../../../../services/file.service';

@Component({
  selector: 'app-unit-info',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule],
  templateUrl: './unit-info.component.html',
  styleUrl: './unit-info.component.scss'
})

export class UnitInfoComponent implements OnInit {

  info: any = {
    id: "e502159b-4a47-49df-b818-71e94e3cc167",
    companyName: "CÔNG TY TNHH BAO BÌ VÀ IN PGS VIỆT NHẬT",
    website: null,
    taxCode: "0202029650",
    address: "Xóm 6, Xã Đông Sơn, Huyện Thủy Nguyên, Tp. Hải Phòng",
    email: "baobivainpgsvietnhat@gmail.com",
    logo: "",
    phone: "0983 228 646",
    chief: "Nguyễn Đình Trường",
    creator: "Nguyễn Đình Trường",
    recipient: "Nguyễn Đình Trường",
    warehouseKeeper: "Nguyễn Đình Trường",
    accountant: "Nguyễn Đình Trường",
    qrCode: null
  }

  logo: any;


  constructor(
    private _env: EnvService,
    private _message: UiToastService,
    private _service: SystemService,
    private _sanitizer: DomSanitizer,
    private _file: FileService
  ) { }

  ngOnInit() {
    this.GetInfo();
    
  }

  /** Lưu dữ liệu hệ thống */
  Save() {
    if (this.Validate()) {
      this._service.SetAppConfig(this.info).subscribe((response: any) => {
        if (response.isSuccess)
          this._message.success("Cập nhật thông tin đơn vị thành công.");
      })
    }
  }

  OnChangeLogo(event: any) {
    let file = event.files[0];
    let form = new FormData();
    form.append("file", file);

    this._file.UploadImage(form, 'avatars').subscribe((response: any) => {
      if (response.isSuccess) {
        this._message.success("Tải lên dữ liệu thành công.");
        var files = response.data as any[];
        if (files.length > 0) {
          this.logo = this._sanitizer.bypassSecurityTrustResourceUrl(`${this._env.baseApiUrl}${files[0].url}`);
          this.info.logo = files[0].url;
        }
      }
    })
  }

  /** Lấy nội dung thông tin doanh nghiệp */
  GetInfo() {
    this._service.GetAppConfig().subscribe((response: any) => {
      if (response.isSuccess) {
        this.info = response.data;
        this.logo = this._sanitizer.bypassSecurityTrustResourceUrl(`${this._env.baseApiUrl}${this.info.logo}`);
      }
    })
  }

  /** Kiểm tra dữ liệu đầu vào */
  Validate() {
    if (IsNull(this.info.companyName)) {
      this._message.error("Tên đơn vị không được trống.");
      return false;
    }
    return true;
  }

}

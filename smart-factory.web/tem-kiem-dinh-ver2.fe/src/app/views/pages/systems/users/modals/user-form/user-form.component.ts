import { Component, HostListener, Input, OnInit } from '@angular/core';
import { NgClass, NgIf } from '@angular/common';
import { JwtHelperService } from '@auth0/angular-jwt';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MenuItem } from 'primeng/api';
import * as moment from 'moment';
import { Router } from '@angular/router';
import { PrimengModule } from '../../../../../../primeng.module';
import { SharedModule } from '../../../../../../shared.module';
import { UiModalService } from '../../../../../../services/shared/ui-modal.service';
import { EnvService } from '../../../../../../env.service';
import { DEFAULT_AVATAR, Genders, Roles } from '../../../../../../services/shared/default-data';
import { UiToastService } from '../../../../../../services/shared/ui-toast.service';
import { IsNull } from '../../../../../../services/shared/common';
import { isValidEmail } from '../../../../../../shared/SharedFunction';
import { TextGlobalConstants } from '../../../../../../shared/TextGlobalContants';
import { UserService } from '../../../../../../services/system/user.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})

export class UserFormComponent implements OnInit {

  roles: any[] = Roles;

  genders: any[] = Genders;

  isUpdate: boolean = false;

  @Input() user: any = {
    id: null,
    userName: null,
    email: null,
    avatar: null,
    birthDay: new Date(),
    role: 0,
    gender: 0,
    name: null,
    password: null,
    address: null,
    phone: null,
    detail: null,
    isRootAdmin: false,
    taxCode: null,
    companyName: null
  }

  avatar: any = '../../../../../../../assets/images/avatars/8.jpg';

  constructor(
    private _env: EnvService,
    private _modal: UiModalService,
    private _router: Router,
    private _toastService: UiToastService,
    private _userSerivce: UserService,
    private _confirm: ConfirmationService,
  ) { }

  ngOnInit() {
    if (!IsNull(this.user?.id)) {
      this.isUpdate = true;
      if (IsNull(this.user.avatar)) {
        this.avatar = DEFAULT_AVATAR
      } else {
        this.avatar = `${this._env.baseApiUrl}${this.user.avatar}`;
      }
    }
  }

  Close() {
    this._modal.closeModal();
  }

  ToLabelList() {
    this._router.navigate(['/label/labels']);
    this._modal.closeModal();
  }

  /// Sự kiện cập nhật ảnh đại diện
  OnChangeAvatar(event: any) {
    let file = event.files[0];
    let form = new FormData();
    form.append("file", file);
  }

  Save() {
    if (this.Validate()) {
      let request = JSON.parse(JSON.stringify(this.user));
      request.birthDay = moment(request.birthDay).format(TextGlobalConstants.FORMAT_DATE_REQUEST);

      this._userSerivce.CreateOrUpdateUser(this.user).subscribe((response: any) => {
        if (response.isSuccess) {
          this._toastService.success("Thêm mới người dùng thành công.");
          this._modal.closeModal(true);
        }
      })
    }
  }

  Validate() {

    console.log(this.user);
    let result = true;
    if (IsNull(this.user.name)) {
      this._toastService.error("Tên người dùng không được trống.");
      result = false;
    }

    if (IsNull(this.user.phone)) {
      this._toastService.error("Số điện thoại không được trống.");
      result = false;
    }

    if (IsNull(this.user.userName)) {
      this._toastService.error("Tên đăng nhập không được trống.");
      result = false;
    }
    if (IsNull(this.user.password) && !this.isUpdate) {
      this._toastService.error("Mật khẩu không được trống.");
      result = false;
    }

    if (!IsNull(this.user.email) && !isValidEmail(this.user.email)) {
      this._toastService.error("Thư điện tử không hợp lệ.");
      result = false;
    }

    return result;
  }

  Delete() {
    this._confirm.confirm({
      message: `Hệ thống sẽ xóa hoàn toàn người dùng <strong>${this.user.name}</strong> nhưng vẫn giữ lại các tem, lô tem đã xuất bản sử dụng mẫu này.`,
      header: 'Xác nhận xóa',
      icon: 'none',
      acceptLabel: 'Xóa người dùng',
      rejectLabel: "Hủy bỏ",
      acceptButtonStyleClass: "p-ripple p-element p-button p-component p-button-danger",
      acceptIcon: "none",
      rejectIcon: "none",
      rejectButtonStyleClass: "p-ripple p-element p-button p-component p-button-secondary",
      accept: () => {
        this._userSerivce.DeleteUser(this.user.id).subscribe((response: any) => {
          if (response.isSuccess) {
            this._toastService.success("Đã xóa người dùng thành công");
            this._modal.closeModal(true);
          }
        });
      },
      reject: () => {
      },
      key: 'confirmDetele'
    });
  }



}

import { Component, Input, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { Router } from '@angular/router';
import { PrimengModule } from '../../../../../../primeng.module';
import { SharedModule } from '../../../../../../shared.module';
import { UiModalService } from '../../../../../../services/shared/ui-modal.service';
import { UiToastService } from '../../../../../../services/shared/ui-toast.service';
import { IsNull } from '../../../../../../services/shared/common';
import { isValidEmail } from '../../../../../../shared/SharedFunction';
import { UserService } from '../../../../../../services/system/user.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule,
    NgIf
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})

export class UserFormComponent implements OnInit {

  isUpdate: boolean = false;
  isLoading: boolean = false;

  @Input() user: any = null;

  /// Form data
  formData: any = {
    id: null,
    email: '',
    fullName: '',
    phoneNumber: '',
    password: '',
    isActive: true
  };

  constructor(
    private _modal: UiModalService,
    private _router: Router,
    private _toastService: UiToastService,
    private _userService: UserService,
    private _confirm: ConfirmationService,
  ) { }

  ngOnInit() {
    if (this.user && this.user.id) {
      this.isUpdate = true;
      this.formData = {
        id: this.user.id,
        email: this.user.email,
        fullName: this.user.fullName,
        phoneNumber: this.user.phoneNumber || '',
        password: '',
        isActive: this.user.isActive
      };
    }
  }

  Close() {
    this._modal.closeModal();
  }

  /// Lưu người dùng (Thêm mới hoặc Cập nhật)
  Save() {
    if (!this.Validate()) return;

    this.isLoading = true;

    if (this.isUpdate) {
      // Cập nhật người dùng
      const updateData: any = {
        fullName: this.formData.fullName,
        phoneNumber: this.formData.phoneNumber || null,
        isActive: this.formData.isActive
      };

      // Chỉ gửi password nếu có nhập
      if (!IsNull(this.formData.password)) {
        updateData.password = this.formData.password;
      }

      this._userService.UpdateUser(this.formData.id, updateData).subscribe({
        next: (response: any) => {
          this._toastService.success("Cập nhật người dùng thành công");
          this._modal.closeModal(true);
          this.isLoading = false;
        },
        error: (err: any) => {
          this._toastService.error(err.error?.message || "Cập nhật người dùng thất bại");
          this.isLoading = false;
        }
      });
    } else {
      // Thêm mới người dùng
      const createData = {
        email: this.formData.email,
        fullName: this.formData.fullName,
        phoneNumber: this.formData.phoneNumber || null,
        password: this.formData.password
      };

      this._userService.CreateUser(createData).subscribe({
        next: (response: any) => {
          this._toastService.success("Thêm mới người dùng thành công");
          this._modal.closeModal(true);
          this.isLoading = false;
        },
        error: (err: any) => {
          this._toastService.error(err.error?.message || "Thêm mới người dùng thất bại");
          this.isLoading = false;
        }
      });
    }
  }

  /// Validate form
  Validate(): boolean {
    if (IsNull(this.formData.fullName)) {
      this._toastService.warning("Họ tên không được để trống");
      return false;
    }

    if (IsNull(this.formData.email)) {
      this._toastService.warning("Email không được để trống");
      return false;
    }

    if (!isValidEmail(this.formData.email)) {
      this._toastService.warning("Email không hợp lệ");
      return false;
    }

    if (!this.isUpdate && IsNull(this.formData.password)) {
      this._toastService.warning("Mật khẩu không được để trống");
      return false;
    }

    if (!IsNull(this.formData.password) && this.formData.password.length < 6) {
      this._toastService.warning("Mật khẩu phải có ít nhất 6 ký tự");
      return false;
    }

    return true;
  }

  /// Xóa người dùng
  Delete() {
    this._confirm.confirm({
      message: `Bạn có chắc chắn muốn xóa người dùng <strong>${this.formData.fullName}</strong>?`,
      header: 'Xác nhận xóa',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Xóa',
      rejectLabel: "Hủy",
      acceptButtonStyleClass: "p-button p-button-danger",
      acceptIcon: "none",
      rejectIcon: "none",
      rejectButtonStyleClass: "p-button p-button-secondary",
      accept: () => {
        this._userService.DeleteUser(this.formData.id).subscribe({
          next: (response: any) => {
            this._toastService.success("Xóa người dùng thành công");
            this._modal.closeModal(true);
          },
          error: (err: any) => {
            this._toastService.error(err.error?.message || "Xóa người dùng thất bại");
          }
        });
      },
      key: 'confirmDelete'
    });
  }
}

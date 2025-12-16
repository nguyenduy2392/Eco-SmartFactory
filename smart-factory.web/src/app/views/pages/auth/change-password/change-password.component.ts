import { Component, Input } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PrimengModule } from '../../../../primeng.module';
import { SharedModule } from '../../../../shared.module';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { UserService } from '../../../../services/system/user.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [PrimengModule, SharedModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})

export class ChangePasswordComponent {
  isLoading: boolean = false;

  changePasswordForm: FormGroup;

  message: any = {
    change_password_success: "Đổi mật khẩu thành công",
    check_retype_password_fail: "Mật khẩu mới nhập lại không trùng khớp",
    change_password_fall: "Đổi mật khẩu thất bại",
  }

  constructor(
    private formBuilder: FormBuilder,
    private userService: UserService,
    private modalService: UiModalService,
    private toastService: UiToastService
  ) { }

  ngOnInit(): void {
    this.changePasswordForm = this.formBuilder.group({
      oldPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
      newPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
      retypeNewPassword: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
    });
  }

  changePassword() {
    this.isLoading = true;

    if (this.changePasswordForm.controls['newPassword'].value == this.changePasswordForm.controls['retypeNewPassword'].value) {
      this.userService.changePassword(this.changePasswordForm.value).subscribe({
        next: (res: any) => {
          console.log(res);
          if (res.isSuccess) {
            this.toastService.showToast({ message: this.message.change_password_success, type: 'success', icon: 'pi pi-check', delay: 4000 });
            this.close();
          } else {
            this.toastService.showToast({ message: res.message, type: 'warn', icon: 'pi pi-ban', delay: 4000 });
          }

          this.isLoading = false;
        },
        error: (err: any) => {
          this.isLoading = false;
          this.toastService.showToast({ message: this.message.change_password_fall, type: 'error', icon: 'pi pi-exclamation-triangle', delay: 4000 });
        },
      });
    }else{
      this.isLoading = false;
      this.toastService.showToast({ message: this.message.check_retype_password_fail, type: 'warn', icon: 'pi pi-exclamation-triangle', delay: 4000 });
    }
  }

  close() {
    this.modalService.closeModal();
  }
}

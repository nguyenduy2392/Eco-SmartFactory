import { Component, OnInit } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../../services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { SharedModule } from '../../../../shared.module';
import { IsNull } from '../../../../services/shared/common';
import { SystemService } from '../../../../services/system/system.service';
import { DomSanitizer } from '@angular/platform-browser';
import { EnvService } from '../../../../env.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [PrimengModule, SharedModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {

  loginForm: any = {
    userName: null,
    password: null
  }

  logo: any;
  showPassword: boolean = false;
  rememberMe: boolean = true;

  images: any[] = [
    "https://khudothivsip-haiphong.com.vn/wp-content/uploads/2020/08/cau-hoang-van-thu-12.jpg",
    "https://khudothivsip-haiphong.com.vn/wp-content/uploads/2020/08/cau-hoang-van-thu-12.jpg",
    "https://khudothivsip-haiphong.com.vn/wp-content/uploads/2020/08/cau-hoang-van-thu-12.jpg"]

  returnUrl: string = '/dashboard';
  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastService: UiToastService,
    private route: ActivatedRoute,
    private system: SystemService,
    private _sanitizer: DomSanitizer,
    private _env: EnvService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      let data = params['returnUrl'];
      if (!IsNull(data)) {
        this.returnUrl = data;
      }
    });

    this.logo = this._sanitizer.bypassSecurityTrustResourceUrl(`${this._env.baseApiUrl}files/assets/logo.png`);
    if (this.authService.IsLoggedIn()) {
      this.router.navigate([this.returnUrl]);
    }
  }


  /** Đăng nhập hệ thống */
  login() {
    if (IsNull(this.loginForm.userName)) {
      this.toastService.warning("Tên đăng nhập không được trống.");
      return;
    }
    if (IsNull(this.loginForm.password)) {
      this.toastService.warning("Mật khẩu không được trống.");
      return;
    }

    this.authService.login(this.loginForm).subscribe((res: any) => {
      if (res.isSuccess) {
        /// Lưu thông tin người dùng
        localStorage.setItem('user', JSON.stringify(res.data.user));
        
        /// Lưu token
        localStorage.setItem('token', res.data.token);

        this.router.navigate([this.returnUrl]);
        this.toastService.success("Đăng nhập thành công");

      } else {
        this.toastService.error(res.message);
      }
    });
  }
}

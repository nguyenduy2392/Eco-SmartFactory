import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { LayoutService } from 'src/app/layout/service/app.layout.service';
import { PrimengModule } from 'src/app/primeng.module';
import { AuthService } from 'src/app/services/auth.service';
import { IsNull } from 'src/app/services/shared/common';
import { UiToastService } from 'src/app/services/shared/ui-toast.service';
import { SharedModule } from 'src/app/shared.module';

@Component({
    standalone: true,
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss',
    imports: [SharedModule, PrimengModule]
})
export class LoginComponent {
  loginForm: any = {
    email: null,
    password: null
  }

  returnUrl: string = '/dashboard';
  constructor(
    private formBuilder: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastService: UiToastService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {

      let data = params['returnUrl'];
      if (!IsNull(data)) {
        this.returnUrl = data;
      }
    });

    if (this.authService.IsLoggedIn()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  /// login
  login() {
    if (IsNull(this.loginForm.email)) {
      this.toastService.warning("Email không được trống.");
      return;
    }
    if (IsNull(this.loginForm.password)) {
      this.toastService.warning("Mật khẩu không được trống.");
      return;
    }

    this.authService.login(this.loginForm).subscribe({
      next: (res: any) => {
        /// Lưu thông tin người dùng
        localStorage.setItem('user', JSON.stringify(res.user));

        /// Lưu token
        localStorage.setItem('token', res.token);

        /// Di chuyển đến trang tìm kiếm nếu người dùng quét
        if (this.returnUrl.includes('tim-kiem')) {
          const urlObj = new URL('http://localhost' + this.returnUrl);
          const params = new URLSearchParams(urlObj.search);

          const database = params.get('database');
          const imei = params.get('imei');

          this.router.navigate(['/tim-kiem'], { queryParams: { database: database, imei: imei } })

        } else {
          this.router.navigate([this.returnUrl]);
        }
        this.toastService.success("Đăng nhập thành công");
      },
      error: (err: any) => {
        this.toastService.error(err.error?.message || "Email hoặc mật khẩu không đúng");
      }
    });
  }
}

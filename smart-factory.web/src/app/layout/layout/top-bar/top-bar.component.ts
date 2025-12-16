import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PrimengModule } from '../../../primeng.module';
import { MenuItem } from 'primeng/api';
import { Router } from '@angular/router';
import { UiModal } from '../../../models/interface/uiInterface';
import { UiModalService } from '../../../services/shared/ui-modal.service';
import { ChangePasswordComponent } from '../../../views/pages/auth/change-password/change-password.component';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [CommonModule, PrimengModule],
  templateUrl: './top-bar.component.html',
  styleUrl: './top-bar.component.scss'
})
export class TopBarComponent {
  userMenus: MenuItem[] | undefined;

  constructor(
    private router: Router,
    private _modal: UiModalService
  ) { }

  ngOnInit() {
    this.userMenus = [
      {
        label: 'Cài đặt',
        items: [
          {
            label: 'Đổi mật khẩu',
            icon: 'pi pi-lock-open',
            command: () => {
              this.openChangePassword();
            }
          },
          {
            label: 'Đăng xuất',
            icon: 'pi pi-sign-out',
            command: () => {
              this.logout();
            }
          }
        ]
      }
    ];
  }

  /** Đăng xuất khỏi hệ thống */
  logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  openChangePassword() {
    const modalOptions: UiModal = {
      title: 'Đổi mật khẩu',
      bodyComponent: ChangePasswordComponent,
      showFooter: false,
      size: '30vw',
    };
    this._modal.create(modalOptions);
  }
}

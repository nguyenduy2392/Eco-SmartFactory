import { Component } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SharedModule } from '../../../../shared.module';
import moment from 'moment';
import { Router } from '@angular/router';
import { UiModal } from '../../../../models/interface/uiInterface';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { UserFormComponent } from './modals/user-form/user-form.component';
import { UserService } from '../../../../services/system/user.service';
import { DEFAULT_AVATAR } from '../../../../services/shared/default-data';
import { IsNull } from '../../../../services/shared/common';
import { EnvService } from '../../../../env.service';

@Component({
  selector: 'app-label-publish',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {

  private jwtHelper = new JwtHelperService();

  isLoading: boolean = false;
  /// bộ lọc
  filter: any = {
    keyword: null,
    role: null,
  }

  /// Danh sách người dùng
  users: any = [];

  /// Tổng số lượng
  total: any = 0;

  constructor(
    private _toastService: UiToastService,
    private _modalService: UiModalService,
    private _service: UserService,
    private _router: Router,
    private _env: EnvService
  ) { }

  ngOnInit() {
    this.GetAllUser();
  }

  /** Lấy danh sách người dùng hệ thống */
  GetAllUser() {
    this._service.GetAllUser(this.filter).subscribe((response: any) => {
      if (response.isSuccess) {
        this.users = response.data as any[];
        this.users.forEach(element => {
          if (!IsNull(element.avatar)) {
            element.avatarUrl = `${this._env.baseApiUrl}${element.avatar}`
          } else {
            element.avatarUrl = `${DEFAULT_AVATAR}`
          }
        })
      }
    })
  }

  /** Mở cửa sổ thêm mới người dùng */
  AddUser(item?: any) {
    const modalOptions: UiModal = {
      title: item == null ? 'Thêm mới người dùng' : 'Cập nhật người dùng',
      bodyComponent: UserFormComponent,
      bodyData: {
      },
      showFooter: false,
      size: '60vw',
    };
    const modal = this._modalService.create(modalOptions);
    modal.afterClose.subscribe(() => this.GetAllUser());
  }

  /** Chi tiết người dùng */
  DetailUser(item?: any) {
    const modalOptions: UiModal = {
      title: item == null ? 'Thêm mới người dùng' : 'Cập nhật người dùng',
      bodyComponent: UserFormComponent,
      bodyData: {
        user: item
      },
      showFooter: false,
      size: '60vw',
    };
    const modal = this._modalService.create(modalOptions);

    modal.afterClose.subscribe((response: any) => {
      if (response) this.GetAllUser();
    })
  }
}

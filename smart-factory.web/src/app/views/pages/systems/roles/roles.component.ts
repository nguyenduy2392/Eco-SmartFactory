import { Component } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { NgClass, NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import * as moment from 'moment';
import { TextGlobalConstants } from '../../../../shared/TextGlobalContants';
import { GenerateFile } from '../../../../shared/helper';
import { Router } from '@angular/router';
import { UiModal } from '../../../../models/interface/uiInterface';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { UserService } from '../../../../services/system/user.service';
import { DEFAULT_AVATAR, Roles } from '../../../../services/shared/default-data';
import { IsNull } from '../../../../services/shared/common';
import { EnvService } from '../../../../env.service';
import { SystemService } from '../../../../services/system/system.service';
import { RoleFormComponent } from './modals/role-form/role-form.component';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    PrimengModule,
    NgClass,
    NgIf,
    SharedModule
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent {

  private jwtHelper = new JwtHelperService();

  isLoading: boolean = false;

  /// bộ lọc
  filter: any = {
    keyword: null,
  }

  /// Danh sách người dùng
  roles: any = [];

  /// Tổng số lượng
  total: any = 0;

  /// Từ khóa tìm kiếm
  keyword: string = null;

  constructor(
    private _toastService: UiToastService,
    private _modalService: UiModalService,
    private _service: SystemService,
    private _router: Router,
    private _env: EnvService
  ) { }

  ngOnInit() {
    this.GetAllRoles();
  }

  GetAllRoles() {
    this._service.GetAllRole(this.keyword).subscribe((response: any) => {
      if (response.isSuccess) {
        this.roles = response.data as any[];
      }
    })
  }

  /// Thêm mới vai trò
  CreateRole() {
    const modalOptions: UiModal = {
      title: `Thêm mới vai trò`,
      bodyComponent: RoleFormComponent,
      bodyData: {
      },
      maximize: false,
      showFooter: false,
      size: '70vw',
    };
    const modal = this._modalService.create(modalOptions);
  }

  /// Chi tiết nhóm quyền
  DetailRole(item?: any) {
    const modalOptions: UiModal = {
      title: item == null ? 'Thêm mới vai trò' : 'Cập nhật vai trò',
      bodyComponent: RoleFormComponent,
      bodyData: {
        id: item.id
      },
      showFooter: false,
      size: '60vw',
    };
    const modal = this._modalService.create(modalOptions);

    modal.afterClose.subscribe((response: any) => {
      if (response) this.GetAllRoles();
    })
  }
}

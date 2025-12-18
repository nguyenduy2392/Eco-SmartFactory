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
import { SystemService } from '../../../../../../services/system/system.service';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.scss'
})

export class RoleFormComponent implements OnInit {
  @Input() id?: string = null;

  isUpdate: boolean = false;
  role: any = {
    id: null,
    name: null,
    isDefault: false,
    description: null,
    permissions: [],
    users: []
  }

  constructor(
    private _env: EnvService,
    private _system: SystemService,
    private _modal: UiModalService,
    private _router: Router,
    private _toastService: UiToastService,
    private _confirm: ConfirmationService,
  ) { }

  ngOnInit() {
    if (IsNull(this.id)) this.isUpdate = false;
    else this.isUpdate = true;

    this.GetRole();
  }

  GetRole() {
    this._system.GetRoleDetail(this.id).subscribe((response: any) => {
      this.role = response.data;
      this.role.permissions = response.data.permissions as any[];
      this.role.users = response.data.users as any[];
    });
  }

  Close() {
    this._modal.closeModal();
  }

  Save() {
    if (IsNull(this.role.name)) {
      this._toastService.error("Tên vai trò không được trống.");
      return;
    }

    if (!this.role.permissions.some(x => x.isHas)) {
      this._toastService.error("Cần chọn ít nhất một quyền cho vai trò");
    }

    this._system.CreateOrUpdate(this.role).subscribe((response: any) => {
      if (response.isSuccess) {
        if (this.isUpdate)
          this._toastService.success("Cập nhật vai trò thành công.");
        else
          this._toastService.success("Thêm mới vai trò thành công");

        this._modal.closeModal(true);
      } else {
        this._toastService.error(response.message);
      }
    })
  }


}

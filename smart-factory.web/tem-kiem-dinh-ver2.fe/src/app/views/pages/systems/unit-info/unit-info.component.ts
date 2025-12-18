import { Component, HostListener, OnInit } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { NgClass, NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import { JwtHelperService } from '@auth0/angular-jwt';
import { UiConfirm, UiModal } from '../../../../models/interface/uiInterface';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MenuItem } from 'primeng/api';
import * as moment from 'moment';
import { EnvService } from '../../../../env.service';
import { GenerateFile } from '../../../../shared/helper';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { IsNull } from '../../../../services/shared/common';
import { DataTypes, FontSizes, FontStyles, InspectionTypes, Symbols, Units } from '../../../../services/shared/default-data';
import { Router } from '@angular/router';
import { UiConfirmService } from '../../../../services/shared/ui-confirm.service';
import { TextGlobalConstants } from '../../../../shared/TextGlobalContants';
import { SystemService } from '../../../../services/system/system.service';

@Component({
  selector: 'app-unit-info',
  standalone: true,
  imports: [
    PrimengModule,
    NgClass,
    NgIf,
    SharedModule],
  templateUrl: './unit-info.component.html',
  styleUrl: './unit-info.component.scss'
})

export class UnitInfoComponent implements OnInit {

  info: any = {
    database: null,
    taxCode: null,
    companyName: null,
    address: null,
    phone: null,
    surrogate: null,
    surrogatePosition: null,
    logo: null
  }


  constructor(
    private _env: EnvService,
    private _message: UiToastService,
    private _confirmService: UiConfirmService,
    private _service: SystemService,
    private _router: Router
  ) { }

  ngOnInit() {
    this.GetInfo();
  }

  Save() {
    if (this.Validate()) {
      this._service.SetAppConfig(this.info).subscribe((response: any) => {
        if (response.isSuccess)
          this._message.success("Cập nhật thông tin đơn vị thành công.");
      })
    }
  }

  GetInfo() {
    this._service.GetAppConfig().subscribe((response: any) => {
      if (response.isSuccess) {
        this.info = response.data;
      }
    })
  }


  Validate() {
    if (IsNull(this.info.companyName)) {
      this._message.error("Tên đơn vị không được trống.");
      return false;
    }

    return true;
  }

}

import { Component } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { NgClass, NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import moment from 'moment';
import { TextGlobalConstants } from '../../../../shared/TextGlobalContants';
import { GenerateFile } from '../../../../shared/helper';
import { Router } from '@angular/router';
import { UiModal } from '../../../../models/interface/uiInterface';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { UserService } from '../../../../services/system/user.service';
import { SystemService } from '../../../../services/system/system.service';

@Component({
  selector: 'app-label-publish',
  standalone: true,
  imports: [
    PrimengModule,
    SharedModule
  ],
  templateUrl: './system-log.component.html',
  styleUrl: './system-log.component.scss'
})
export class SystemLogComponent {

  isLoading: boolean = true;
  /// bộ lọc
  filter: any = {
    keyword: null,
    fromDate: null,
    toDate: null,
    userId: null,
    page: 1,
    pageSize: 10
  }

  /// Danh sách lô xuất tem
  histories: any = [];

  /// Tổng số lượng
  total: any = 0;

  users: any[] = [];

  constructor(
    private _toastService: UiToastService,
    private _modalService: UiModalService,
    private _service: SystemService,
    private _userService: UserService,
    private _router: Router
  ) { }

  ngOnInit() {
    this.filter.toDate = moment().toDate();
    this.filter.fromDate = moment().startOf('years').toDate();

    this.GetAllUser();

    this.GetHistory();
  }

  GetAllUser() {
    this._userService.GetAllUser({ role: null }).subscribe((response: any) => {
      if (response.isSuccess) {
        this.users = response.data as any[];
      }
    })
  }

  OnChangePage(event: any) {

  }

  /// Danh sách đợt xuất bản
  GetHistory() {
    this.isLoading = true;
    let request = JSON.parse(JSON.stringify(this.filter));
    request.fromDate = moment(request.fromDate).format(TextGlobalConstants.FORMAT_DATE_REQUEST);
    request.toDate = moment(request.toDate).format(TextGlobalConstants.FORMAT_DATE_REQUEST);

    this._service.GetAuditLog(request).subscribe((response: any) => {
      this.isLoading = false;
      this.histories = response.data.items as any[];
      this.total = response.data.totalCount;
    })

  }

}

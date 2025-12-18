import { Component } from '@angular/core';
import { PrimengModule } from '../../../../primeng.module';
import { UiToastService } from '../../../../services/shared/ui-toast.service';
import { NgIf } from '@angular/common';
import { SharedModule } from '../../../../shared.module';
import { Router } from '@angular/router';
import { UiModal } from '../../../../models/interface/uiInterface';
import { UiModalService } from '../../../../services/shared/ui-modal.service';
import { UserFormComponent } from './modals/user-form/user-form.component';
import { UserService } from '../../../../services/system/user.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    PrimengModule,
    NgIf,
    SharedModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent {

  isLoading: boolean = false;

  /// Bộ lọc
  filter: any = {
    keyword: null,
    isActive: null,
  }

  /// Trạng thái người dùng
  statuses: any[] = [
    { name: "Tất cả", value: null },
    { name: "Đang hoạt động", value: true },
    { name: "Đã khóa", value: false }
  ]

  /// Danh sách người dùng
  users: any[] = [];

  /// Tổng số lượng
  total: number = 0;

  constructor(
    private _toastService: UiToastService,
    private _modalService: UiModalService,
    private _service: UserService,
    private _router: Router
  ) { }

  ngOnInit() {
    this.GetAllUsers();
  }

  /// Lấy danh sách người dùng
  GetAllUsers() {
    this.isLoading = true;
    this._service.GetAllUsers(this.filter.keyword, this.filter.isActive).subscribe({
      next: (response: any) => {
        this.users = response;
        this.total = this.users.length;
        this.isLoading = false;
      },
      error: (err: any) => {
        this._toastService.error(err.error?.message || "Không thể tải danh sách người dùng");
        this.isLoading = false;
      }
    });
  }

  /// Thêm mới người dùng
  AddUser() {
    const modalOptions: UiModal = {
      title: 'Thêm mới người dùng',
      bodyComponent: UserFormComponent,
      bodyData: {
        user: null
      },
      showFooter: false,
      size: '60vw',
    };
    const modal = this._modalService.create(modalOptions);
    modal.afterClose.subscribe((result: any) => {
      if (result) this.GetAllUsers();
    });
  }

  /// Chi tiết / Sửa người dùng
  EditUser(item: any) {
    const modalOptions: UiModal = {
      title: 'Cập nhật người dùng',
      bodyComponent: UserFormComponent,
      bodyData: {
        user: { ...item }
      },
      showFooter: false,
      size: '60vw',
    };
    const modal = this._modalService.create(modalOptions);

    modal.afterClose.subscribe((result: any) => {
      if (result) this.GetAllUsers();
    });
  }

  /// Làm mới danh sách
  Refresh() {
    this.filter = {
      keyword: null,
      isActive: null
    };
    this.GetAllUsers();
  }
}

import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'users',
    loadComponent: () => import('./users/users.component').then(c => c.UsersComponent),
    data: {
      title: 'Danh sách người dùng'
    }
  },
  {
    path: 'unit-info',
    loadComponent: () => import('./unit-info/unit-info.component').then(c => c.UnitInfoComponent),
    data: {
      title: 'Thông tin đơn vị'
    }
  },
  {
    path: 'roles',
    loadComponent: () => import('./roles/roles.component').then(c => c.RolesComponent),
    data: {
      title: 'Vai trò hệ thống'
    }
  },
  {
    path: 'history',
    loadComponent: () => import('./system-log/system-log.component').then(c => c.SystemLogComponent),
    data: {
      title: 'Nhật ký hệ thống'
    }
  }
];

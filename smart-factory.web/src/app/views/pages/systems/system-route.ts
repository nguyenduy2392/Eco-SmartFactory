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
    path: 'guide',
    loadComponent: () => import('./guide/guide.component').then(c => c.GuideComponent),
    data: {
      title: 'Hướng dẫn sử dụng'
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
    path: 'history',
    loadComponent: () => import('./system-log/system-log.component').then(c => c.SystemLogComponent),
    data: {
      title: 'Nhật ký hệ thống'
    }
  }
];

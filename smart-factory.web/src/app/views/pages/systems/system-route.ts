import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'users',
    loadComponent: () => import('./users/users.component').then(c => c.UsersComponent),
    data: {
      title: 'Danh sách người dùng',
      breadcrumb: 'Người dùng',
      icon: 'pi pi-users'
    }
  },
  {
    path: 'unit-info',
    loadComponent: () => import('./unit-info/unit-info.component').then(c => c.UnitInfoComponent),
    data: {
      title: 'Thông tin đơn vị',
      breadcrumb: 'Thông tin đơn vị',
      icon: 'pi pi-building'
    }
  },
  {
    path: 'roles',
    loadComponent: () => import('./roles/roles.component').then(c => c.RolesComponent),
    data: {
      title: 'Vai trò hệ thống',
      breadcrumb: 'Vai trò',
      icon: 'pi pi-shield'
    }
  }
];

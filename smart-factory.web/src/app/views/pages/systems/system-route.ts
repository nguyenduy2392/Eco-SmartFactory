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
  },
  {
    path: 'materials',
    loadComponent: () => import('../materials/materials.component').then(c => c.MaterialsComponent),
    data: {
      title: 'Quản lý nguyên vật liệu',
      breadcrumb: 'Nguyên vật liệu',
      icon: 'pi pi-box'
    }
  },
  {
    path: 'units-of-measure',
    loadComponent: () => import('./units-of-measure/units-of-measure.component').then(c => c.UnitsOfMeasureComponent),
    data: {
      title: 'Quản lý đơn vị tính',
      breadcrumb: 'Đơn vị tính',
      icon: 'pi pi-calculator'
    }
  }
];

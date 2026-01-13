import { Routes } from '@angular/router';
import { DashboardComponent } from './views/pages/dashboard/dashboard.component';
import { AuthGuard } from './services/auth.guard';
import { AppLayoutComponent } from './layout/app.layout.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },

  {
    path: '',
    component: AppLayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    children: [
      { 
        path: 'dashboard', 
        component: DashboardComponent, 
        pathMatch: 'full',
        data: {
          title: 'Tổng quan',
          breadcrumb: 'Tổng quan',
          icon: 'pi pi-home'
        }
      },
      {
        path: 'system',
        data: {
          title: 'Hệ thống',
          breadcrumb: 'Hệ thống',
          icon: 'pi pi-cog'
        },
        loadChildren: () => import('./views/pages/systems/system-route').then((m) => m.routes)
      },
      {
        path: 'customers',
        data: {
          title: 'Quản lý chủ hàng',
          breadcrumb: 'Chủ hàng',
          icon: 'pi pi-building'
        },
        children: [
          {
            path: '',
            loadComponent: () => import('./views/pages/customers/customers.component').then(m => m.CustomersComponent),
            data: {
              title: 'Danh sách chủ hàng',
              breadcrumb: 'Danh sách'
            }
          },
          {
            path: ':id',
            loadComponent: () => import('./views/pages/customers/customer-detail/customer-detail.component').then(m => m.CustomerDetailComponent),
            data: {
              title: 'Chi tiết chủ hàng',
              breadcrumb: 'Chi tiết'
            }
          }
        ]
      },
      {
        path: 'processing-po',
        data: {
          title: 'Quản lý đơn hàng gia công',
          breadcrumb: 'Đơn hàng gia công',
          icon: 'pi pi-shopping-cart'
        },
        children: [
          {
            path: '',
            loadComponent: () => import('./components/processing-po/po-list/po-list.component').then(m => m.POListComponent),
            data: {
              title: 'Danh sách đơn hàng',
              breadcrumb: 'Danh sách'
            }
          },
          {
            path: ':id',
            loadComponent: () => import('./components/processing-po/po-detail/po-detail.component').then(m => m.PODetailComponent),
            data: {
              title: 'Chi tiết đơn hàng',
              breadcrumb: 'Chi tiết'
            }
          }
        ]
      },
      {
        path: 'warehouse',
        loadComponent: () => import('./views/pages/warehouse/warehouse.component').then(m => m.WarehouseComponent),
        data: {
          title: 'Quản lý kho nguyên vật liệu',
          breadcrumb: 'Kho nguyên vật liệu',
          icon: 'pi pi-box'
        }
      },
      {
        path: 'stock-in',
        loadComponent: () => import('./components/stock-in/stock-in/stock-in.component').then(m => m.StockInComponent),
        data: {
          title: 'Nhập kho nguyên vật liệu',
          breadcrumb: 'Nhập kho',
          icon: 'pi pi-inbox'
        }
      },
      {
        path: 'production-planning',
        loadComponent: () => import('./views/pages/pmc/production-planning.component').then(m => m.ProductionPlanningComponent),
        data: {
          title: 'Kế hoạch sản xuất chi tiết',
          breadcrumb: 'Kế hoạch chi tiết',
          icon: 'pi pi-table'
        }
      },
      {
        path: 'process-bom',
        loadComponent: () => import('./views/pages/process-bom/process-bom-page.component').then(m => m.ProcessBomPageComponent),
        data: {
          title: 'Quản lý Process BOM',
          breadcrumb: 'Process BOM',
          icon: 'pi pi-sitemap'
        }
      },
    ]
  },
  {
    path: '404',
    loadComponent: () => import('./views/pages/page404/page404.component').then(m => m.Page404Component),
    data: {
      title: 'Page 404'
    }
  },
  {
    path: '500',
    loadComponent: () => import('./views/pages/page500/page500.component').then(m => m.Page500Component),
    data: {
      title: 'Page 500'
    }
  },
  {
    path: 'login',
    loadComponent: () => import('./views/auth/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Đăng nhập'
    }
  },
  {
    path: 'auth',
    loadComponent: () => import('./views/auth/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Đăng nhập'
    }
  },

  { path: '**', redirectTo: 'dashboard' }

];

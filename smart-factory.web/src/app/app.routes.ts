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
        path: 'purchase-orders',
        data: {
          title: 'Quản lý PO',
          breadcrumb: 'Quản lý PO',
          icon: 'pi pi-shopping-cart'
        },
        children: [
          {
            path: '',
            loadComponent: () => import('./components/purchase-orders/po-list/po-list.component').then(m => m.POListComponent),
            data: {
              title: 'Danh sách PO',
              breadcrumb: 'Danh sách'
            }
          },
          {
            path: ':id',
            loadComponent: () => import('./components/purchase-orders/po-detail/po-detail.component').then(m => m.PODetailComponent),
            data: {
              title: 'Chi tiết PO',
              breadcrumb: 'Chi tiết'
            }
          },
          {
            path: ':poId/products/:productId',
            loadComponent: () => import('./components/purchase-orders/product-detail/product-detail.component').then(m => m.ProductDetailComponent),
            data: {
              title: 'Chi tiết Sản phẩm',
              breadcrumb: 'Chi tiết Sản phẩm'
            }
          },
          {
            path: ':poId/products/:productId/components/:componentId',
            loadComponent: () => import('./components/purchase-orders/part-detail/part-detail.component').then(m => m.PartDetailComponent),
            data: {
              title: 'Chi tiết Linh kiện',
              breadcrumb: 'Chi tiết Linh kiện'
            }
          }
        ]
      },
      {
        path: 'customers',
        loadComponent: () => import('./views/pages/customers/customers.component').then(m => m.CustomersComponent),
        data: {
          title: 'Quản lý chủ hàng',
          breadcrumb: 'Chủ hàng',
          icon: 'pi pi-building'
        }
      },
      {
        path: 'materials',
        loadComponent: () => import('./views/pages/materials/materials.component').then(m => m.MaterialsComponent),
        data: {
          title: 'Quản lý vật tư',
          breadcrumb: 'Vật tư',
          icon: 'pi pi-box'
        }
      },
      {
        path: 'tools',
        loadComponent: () => import('./views/pages/tools/tools.component').then(m => m.ToolsComponent),
        data: {
          title: 'Quản lý Tool & Khuôn',
          breadcrumb: 'Tool & Khuôn',
          icon: 'pi pi-wrench'
        }
      },
      {
        path: 'products',
        loadComponent: () => import('./views/pages/products/products.component').then(m => m.ProductsComponent),
        data: {
          title: 'Quản lý sản phẩm',
          breadcrumb: 'Sản phẩm',
          icon: 'pi pi-box'
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

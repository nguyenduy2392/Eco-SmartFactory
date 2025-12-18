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

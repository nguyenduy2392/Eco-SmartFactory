import { Routes } from '@angular/router';
import { DashboardComponent } from './views/pages/dashboard/dashboard.component';
import { AuthGuard } from './services/auth.guard';
import { LayoutComponent } from './layout/layout/layout.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    canActivateChild: [AuthGuard],
    data: {
      title: 'Home'
    },
    children: [

      // Trang chủ
      { path: 'dashboard', component: DashboardComponent, pathMatch: 'full' },

      /// Các tùy chỉnh hệ thống
      {
        path: 'system',
        loadChildren: () => import('./views/pages/systems/system-route').then((m) => m.routes)
      },
    ]
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
    loadComponent: () => import('./views/pages/auth/login/login.component').then(m => m.LoginComponent),
    data: {
      title: 'Login page'
    }
  },

  { path: '**', redirectTo: 'dashboard' }

];

import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot, CanActivateChild, UrlTree, GuardResult, MaybeAsync } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { NzMessageModule, NzMessageService } from 'ng-zorro-antd/message';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate, CanActivateChild {

  constructor(
    private router: Router,
    private message: NzMessageService,
    private authService: AuthService
  ) { }
  canActivateChild(childRoute: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return true
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.IsLoggedIn()) {
      return true;
    }
    this.message.remove();
    // this.message.error('Bạn cần phải đăng nhập');
    this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

}

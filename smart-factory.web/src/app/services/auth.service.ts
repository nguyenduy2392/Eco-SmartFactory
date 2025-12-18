import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { map } from 'rxjs';
import { Constant } from '../constant/Constant';
import { MasterService } from './master/master.service';
import { EnvService } from '../env.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  public apiURL = this.env.baseApiUrl;
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  getTesst() {
    return this.master.get(`${this.apiURL}WeatherForecast`);
  }

  /// Login
  login(data: any) {
    return this.master.post(`${this.apiURL}api/auth/login`, data);
  }

  IsLoggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }
}

import { Injectable } from '@angular/core';
import { MasterService } from '../master/master.service';
import { EnvService } from '../../env.service';
import { IsNull, ToUrlParam } from '../shared/common';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  public apiURL = this.env.baseApiUrl;
  decodedToken: any;
  currentUser: any;

  constructor(
    private master: MasterService,
    private env: EnvService,
  ) { }

  /// Màn hình chính
  GetDashboard() {
    return this.master.get(`${this.apiURL}api/report/dashboard`);
  }
}

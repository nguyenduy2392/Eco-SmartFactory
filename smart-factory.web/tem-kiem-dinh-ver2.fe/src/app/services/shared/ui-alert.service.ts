import { Injectable } from '@angular/core';
import { UiAlert } from '../../models/interface/uiInterface';
import { Subject } from 'rxjs';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class UiAlertService {

  constructor(private messageService: MessageService) { }

  private alertSubject = new Subject<UiAlert>();
  alertState = this.alertSubject.asObservable();

  //đang xem xét giao diện alert
  showAlert(alertOptions: UiAlert) {
    // this.messageService.add({
    //   severity:alertOptions.type,
    //   detail: alertOptions.message,
    //   closable: true,
    //   life: 3500,
    // });
    console.log('Fixing bug');
  }
}

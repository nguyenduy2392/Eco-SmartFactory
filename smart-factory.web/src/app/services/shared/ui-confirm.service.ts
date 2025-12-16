import { Injectable } from '@angular/core';
import { UiConfirm } from '../../models/interface/uiInterface';
import { ConfirmationService } from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class UiConfirmService {

  constructor(private confirmationService: ConfirmationService) { }

  showConfirm(confirm: UiConfirm) {
    if (!this.confirmationService) {
      console.error('Confirm component is not initialized.');
      return;
    }
    const {
      message,
      title = '',
      type = 'info',
      icon = 'pi pi-question-circle',
      confirmButtonText = 'Yes',
      cancelButtonText = 'No',
      confirmButtonIcon = 'pi pi-check',
      cancelButtonIcon = 'pi pi-times',
      onCancelAction,
      onConfirmAction
    } = confirm;
    this.confirmationService.confirm({
      header: title,
      message: message,
      icon: icon + " mr-3",
      acceptIcon: confirmButtonIcon + " mr-2",
      rejectIcon: cancelButtonIcon + " mr-2",
      rejectButtonStyleClass: 'p-button-outlined p-button-md mr-3 text-center',
      acceptButtonStyleClass: 'p-button-md mr-3 text-center',
      acceptLabel: confirmButtonText,
      rejectLabel: cancelButtonText,
      accept: onConfirmAction,
      reject: onCancelAction
    });
  }

  close() {
    if (!this.confirmationService) {
      console.error('Confirm component is not initialized.');
      return;
    }
    this.confirmationService.close();
  }
}

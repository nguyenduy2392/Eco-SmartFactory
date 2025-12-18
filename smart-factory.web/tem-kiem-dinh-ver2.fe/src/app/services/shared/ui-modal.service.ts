import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { UiModal } from '../../models/interface/uiInterface';

@Injectable({
  providedIn: 'root'
})
export class UiModalService {
  private visibleSubject = new BehaviorSubject<boolean>(false);
  visible$ = this.visibleSubject.asObservable();

  private modalSubject = new BehaviorSubject<UiModal | null>(null);
  modal$ = this.modalSubject.asObservable();

  private afterOpenSubject = new Subject<void>();
  afterOpen$ = this.afterOpenSubject.asObservable();

  private afterCloseSubject = new Subject<any>();

  openModal(modal: UiModal) {
    this.modalSubject.next(modal);
    this.visibleSubject.next(true);
  }

  closeModal(result?: any) {
    this.visibleSubject.next(false);
    this.afterCloseSubject.next(result);
  }

  create(modalOptions: UiModal): { afterClose: Observable<any> } {
    this.modalSubject.next(modalOptions);
    this.visibleSubject.next(true);
    this.afterOpenSubject.next();

    return {
      afterClose: this.afterCloseSubject.asObservable()
    };
  }

}

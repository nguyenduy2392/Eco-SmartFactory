/* tslint:disable:no-unused-variable */

import { TestBed, inject } from '@angular/core/testing';
import { UiAlertService } from './ui-alert.service';

describe('Service: UiAlert', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [UiAlertService]
    });
  });

  it('should ...', inject([UiAlertService], (service: UiAlertService) => {
    expect(service).toBeTruthy();
  }));
});

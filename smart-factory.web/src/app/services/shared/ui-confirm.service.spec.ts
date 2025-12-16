import { TestBed } from '@angular/core/testing';

import { UiConfirmService } from './ui-confirm.service';

describe('UiConfirmService', () => {
  let service: UiConfirmService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UiConfirmService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

import { TestBed } from '@angular/core/testing';

import { UiToastService } from './ui-toast.service';

describe('UiToastService', () => {
  let service: UiToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(UiToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

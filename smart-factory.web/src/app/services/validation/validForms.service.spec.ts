/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { ValidFormsService } from './validForms.service';

describe('Service: ValidForms', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ValidFormsService]
    });
  });

  it('should ...', inject([ValidFormsService], (service: ValidFormsService) => {
    expect(service).toBeTruthy();
  }));
});

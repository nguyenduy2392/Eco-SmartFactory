import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UiToastComponent } from './ui-toast.component';

describe('UiToastComponent', () => {
  let component: UiToastComponent;
  let fixture: ComponentFixture<UiToastComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UiToastComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UiToastComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

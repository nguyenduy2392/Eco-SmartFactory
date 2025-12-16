import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DefaultTopbarComponent } from './default-topbar.component';

describe('DefaultTopbarComponent', () => {
  let component: DefaultTopbarComponent;
  let fixture: ComponentFixture<DefaultTopbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DefaultTopbarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DefaultTopbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

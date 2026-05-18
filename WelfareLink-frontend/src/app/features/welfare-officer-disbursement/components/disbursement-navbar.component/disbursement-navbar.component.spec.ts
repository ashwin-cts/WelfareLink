import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DisbursementNavbarComponent } from './disbursement-navbar.component';

describe('DisbursementNavbarComponent', () => {
  let component: DisbursementNavbarComponent;
  let fixture: ComponentFixture<DisbursementNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DisbursementNavbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DisbursementNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

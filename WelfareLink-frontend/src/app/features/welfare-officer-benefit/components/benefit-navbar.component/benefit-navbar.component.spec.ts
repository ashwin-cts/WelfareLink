import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BenefitNavbarComponent } from './benefit-navbar.component';

describe('BenefitNavbarComponent', () => {
  let component: BenefitNavbarComponent;
  let fixture: ComponentFixture<BenefitNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BenefitNavbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BenefitNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

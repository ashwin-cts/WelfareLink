import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BenefitAnalyticsComponent } from './benefit-analytics.component';

describe('BenefitAnalyticsComponent', () => {
  let component: BenefitAnalyticsComponent;
  let fixture: ComponentFixture<BenefitAnalyticsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BenefitAnalyticsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(BenefitAnalyticsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

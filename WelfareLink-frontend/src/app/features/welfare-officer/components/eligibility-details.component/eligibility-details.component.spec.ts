import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EligibilityDetailsComponent } from './eligibility-details.component';

describe('EligibilityDetailsComponent', () => {
  let component: EligibilityDetailsComponent;
  let fixture: ComponentFixture<EligibilityDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EligibilityDetailsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EligibilityDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

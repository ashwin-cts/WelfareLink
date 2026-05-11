import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EligibilityFormComponent } from './eligibility-form.component';

describe('EligibilityFormComponent', () => {
  let component: EligibilityFormComponent;
  let fixture: ComponentFixture<EligibilityFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EligibilityFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EligibilityFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

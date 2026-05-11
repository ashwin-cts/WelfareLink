import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EligibilityEditComponent } from './eligibility-edit.component';

describe('EligibilityEditComponent', () => {
  let component: EligibilityEditComponent;
  let fixture: ComponentFixture<EligibilityEditComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EligibilityEditComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EligibilityEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenApplyFormComponent } from './citizen-apply-form.component';

describe('CitizenApplyFormComponent', () => {
  let component: CitizenApplyFormComponent;
  let fixture: ComponentFixture<CitizenApplyFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitizenApplyFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenApplyFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

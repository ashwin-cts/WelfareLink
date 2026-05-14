import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplianceOfficerProfileComponent } from './compliance-officer-profile.component';

describe('ComplianceOfficerProfileComponent', () => {
  let component: ComplianceOfficerProfileComponent;
  let fixture: ComponentFixture<ComplianceOfficerProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComplianceOfficerProfileComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ComplianceOfficerProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

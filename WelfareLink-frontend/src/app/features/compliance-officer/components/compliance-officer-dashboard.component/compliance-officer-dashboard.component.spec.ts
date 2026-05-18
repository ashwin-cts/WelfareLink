import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplianceOfficerDashboardComponent } from './compliance-officer-dashboard.component';

describe('ComplianceOfficerDashboardComponent', () => {
  let component: ComplianceOfficerDashboardComponent;
  let fixture: ComponentFixture<ComplianceOfficerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComplianceOfficerDashboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ComplianceOfficerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

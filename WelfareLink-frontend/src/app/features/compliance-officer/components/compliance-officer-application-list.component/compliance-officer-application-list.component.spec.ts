import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplianceOfficerApplicationListComponent } from './compliance-officer-application-list.component';

describe('ComplianceOfficerApplicationListComponent', () => {
  let component: ComplianceOfficerApplicationListComponent;
  let fixture: ComponentFixture<ComplianceOfficerApplicationListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComplianceOfficerApplicationListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ComplianceOfficerApplicationListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

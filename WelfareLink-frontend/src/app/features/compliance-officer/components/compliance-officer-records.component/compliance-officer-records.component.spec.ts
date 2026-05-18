import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ComplianceOfficerRecordsComponent } from './compliance-officer-records.component';

describe('ComplianceOfficerRecordsComponent', () => {
  let component: ComplianceOfficerRecordsComponent;
  let fixture: ComponentFixture<ComplianceOfficerRecordsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ComplianceOfficerRecordsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ComplianceOfficerRecordsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

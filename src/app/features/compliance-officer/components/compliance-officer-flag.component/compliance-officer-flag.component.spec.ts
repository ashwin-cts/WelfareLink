import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FlagIssueComponent } from './compliance-officer-flag.component';

describe('ComplianceOfficerFlagComponent', () => {
  let component: FlagIssueComponent;
  let fixture: ComponentFixture<FlagIssueComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FlagIssueComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FlagIssueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

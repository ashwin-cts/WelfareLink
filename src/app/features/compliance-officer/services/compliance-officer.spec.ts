import { TestBed } from '@angular/core/testing';

import { ComplianceOfficerService } from './compliance-officer';

describe('ComplianceOfficer', () => {
  let service: ComplianceOfficerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ComplianceOfficerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

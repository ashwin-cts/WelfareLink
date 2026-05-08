import { TestBed } from '@angular/core/testing';

import { ProgramManagerService } from './program-manager.service';

describe('ProgramManager', () => {
  let service: ProgramManagerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProgramManagerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});

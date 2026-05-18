import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuditorProfileComponent } from './auditor-profile.component';

describe('AuditorProfileComponent', () => {
  let component: AuditorProfileComponent;
  let fixture: ComponentFixture<AuditorProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuditorProfileComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AuditorProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

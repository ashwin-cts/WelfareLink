import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WelfareOfficerProfileComponent } from './welfare-officer-profile.component';

describe('WelfareOfficerProfileComponent', () => {
  let component: WelfareOfficerProfileComponent;
  let fixture: ComponentFixture<WelfareOfficerProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WelfareOfficerProfileComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WelfareOfficerProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

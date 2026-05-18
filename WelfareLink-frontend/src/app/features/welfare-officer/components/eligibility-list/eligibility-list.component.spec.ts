import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EligibilityList } from './eligibility-list.component';

describe('EligibilityList', () => {
  let component: EligibilityList;
  let fixture: ComponentFixture<EligibilityList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EligibilityList],
    }).compileComponents();

    fixture = TestBed.createComponent(EligibilityList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

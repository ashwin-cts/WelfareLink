import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DisbursementListComponent } from './disbursement-list.component';

describe('DisbursementListComponent', () => {
  let component: DisbursementListComponent;
  let fixture: ComponentFixture<DisbursementListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DisbursementListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DisbursementListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

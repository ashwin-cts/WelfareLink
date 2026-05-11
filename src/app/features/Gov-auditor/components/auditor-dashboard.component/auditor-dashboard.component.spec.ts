import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GovDashboardComponent } from './gov-dashboard.component';

describe('GovDashboardComponent', () => {
  let component: GovDashboardComponent;
  let fixture: ComponentFixture<GovDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GovDashboardComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(GovDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

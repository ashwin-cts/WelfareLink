import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WelfareApplicationAnalyticsComponent } from './welfare-application-analytics.component';

describe('WelfareApplicationAnalyticsComponent', () => {
  let component: WelfareApplicationAnalyticsComponent;
  let fixture: ComponentFixture<WelfareApplicationAnalyticsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WelfareApplicationAnalyticsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WelfareApplicationAnalyticsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

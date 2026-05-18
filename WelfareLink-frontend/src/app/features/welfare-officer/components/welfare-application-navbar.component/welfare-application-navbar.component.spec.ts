import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WelfareApplicationNavbarComponent } from './welfare-application-navbar.component';

describe('WelfareApplicationNavbarComponent', () => {
  let component: WelfareApplicationNavbarComponent;
  let fixture: ComponentFixture<WelfareApplicationNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WelfareApplicationNavbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(WelfareApplicationNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

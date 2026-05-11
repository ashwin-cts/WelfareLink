import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenApplicationsComponent } from './citizen-applications.component';

describe('CitizenApplicationsComponent', () => {
  let component: CitizenApplicationsComponent;
  let fixture: ComponentFixture<CitizenApplicationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitizenApplicationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenApplicationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenProgramListComponent } from './citizen-program-list.component';

describe('CitizenProgramsComponent', () => {
  let component: CitizenProgramListComponent;
  let fixture: ComponentFixture<CitizenProgramListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitizenProgramListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenProgramListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

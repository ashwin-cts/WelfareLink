import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProgramManagerNavbarComponent } from './program-manager-navbar.component';

describe('ProgramManagerNavbarComponent', () => {
  let component: ProgramManagerNavbarComponent;
  let fixture: ComponentFixture<ProgramManagerNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProgramManagerNavbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProgramManagerNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

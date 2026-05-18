import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PmProfileComponent } from './pm-profile.component';

describe('PmProfileComponent', () => {
  let component: PmProfileComponent;
  let fixture: ComponentFixture<PmProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PmProfileComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(PmProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

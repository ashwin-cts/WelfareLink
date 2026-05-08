import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ResourceNavbarComponent } from './resource-navbar.component';

describe('ResourceNavbarComponent', () => {
  let component: ResourceNavbarComponent;
  let fixture: ComponentFixture<ResourceNavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResourceNavbarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ResourceNavbarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

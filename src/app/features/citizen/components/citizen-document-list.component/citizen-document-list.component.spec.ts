import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenDocumentListComponent } from './citizen-document-list.component';

describe('CitizenDocumentListComponent', () => {
  let component: CitizenDocumentListComponent;
  let fixture: ComponentFixture<CitizenDocumentListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitizenDocumentListComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenDocumentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

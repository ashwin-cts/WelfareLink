import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CitizenDocumentFormComponent } from './citizen-document-form.component';

describe('CitizenDocumentFormComponent', () => {
  let component: CitizenDocumentFormComponent;
  let fixture: ComponentFixture<CitizenDocumentFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CitizenDocumentFormComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CitizenDocumentFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

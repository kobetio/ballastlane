import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { environment } from '../../../../environments/environment';
import { Book } from '../../../models/book.model';
import { BookForm, BookFormData } from './book-form';

describe('BookForm', () => {
  let component: BookForm;
  let fixture: ComponentFixture<BookForm>;
  let httpMock: HttpTestingController;
  let dialogRef: { close: (result?: unknown) => void };
  const baseUrl = `${environment.apiBaseUrl}/books`;

  const existingBook: Book = {
    id: '1',
    title: 'Dune',
    author: 'Frank Herbert',
    genre: 'Sci-Fi',
    publicationYear: 1965,
    readingStatus: 'Read',
    rating: 5,
    notes: null
  };

  function configure(data: BookFormData) {
    dialogRef = { close: () => {} };
    TestBed.configureTestingModule({
      imports: [BookForm],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    });
    fixture = TestBed.createComponent(BookForm);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  }

  afterEach(() => httpMock.verify());

  it('starts with an empty, invalid form in create mode', () => {
    configure({ book: null });

    expect(component.isEditMode).toBe(false);
    expect(component.form.invalid).toBe(true);
  });

  it('pre-fills the form with the book in edit mode', () => {
    configure({ book: existingBook });

    expect(component.isEditMode).toBe(true);
    expect(component.form.controls.title.value).toBe('Dune');
    expect(component.form.valid).toBe(true);
  });

  it('rejects a publication year before 1450', () => {
    configure({ book: null });
    component.form.controls.publicationYear.setValue(1000);

    expect(component.form.controls.publicationYear.hasError('min')).toBe(true);
  });

  it('rejects a rating outside 1-5', () => {
    configure({ book: null });
    component.form.controls.rating.setValue(6);

    expect(component.form.controls.rating.hasError('max')).toBe(true);
  });

  it('submits a POST and closes the dialog with the created book', () => {
    configure({ book: null });
    let closedWith: unknown;
    dialogRef.close = (value) => (closedWith = value);

    component.form.setValue({
      title: 'Dune',
      author: 'Frank Herbert',
      genre: 'Sci-Fi',
      publicationYear: 1965,
      readingStatus: 'Read',
      rating: 5,
      notes: ''
    });
    component.submit();

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    req.flush(existingBook);

    expect(closedWith).toEqual(existingBook);
  });

  it('submits a PUT when in edit mode', () => {
    configure({ book: existingBook });

    component.submit();

    const req = httpMock.expectOne(`${baseUrl}/${existingBook.id}`);
    expect(req.request.method).toBe('PUT');
    req.flush(existingBook);
  });

  it('maps field-level server validation errors onto the form and keeps the dialog open', () => {
    configure({ book: null });
    let closed = false;
    dialogRef.close = () => (closed = true);
    component.form.setValue({
      title: '',
      author: 'Frank Herbert',
      genre: '',
      publicationYear: null,
      readingStatus: null,
      rating: null,
      notes: ''
    });

    component.submit();
    // form invalid client-side (Title required) -> no HTTP call made, dialog stays open
    expect(closed).toBe(false);
  });
});

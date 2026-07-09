import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { Book, BookRequest } from '../../models/book.model';
import { BookService } from './book.service';

describe('BookService', () => {
  let service: BookService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/books`;

  const sampleBook: Book = {
    id: '1',
    title: 'Dune',
    author: 'Frank Herbert',
    genre: 'Sci-Fi',
    publicationYear: 1965,
    readingStatus: 'Read',
    rating: 5,
    notes: null
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(BookService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getAll sends a GET to /books', () => {
    service.getAll().subscribe((books) => expect(books).toEqual([sampleBook]));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush([sampleBook]);
  });

  it('getById sends a GET to /books/{id}', () => {
    service.getById('1').subscribe((book) => expect(book).toEqual(sampleBook));

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(sampleBook);
  });

  it('create sends a POST to /books', () => {
    const request: BookRequest = {
      title: 'Dune',
      author: 'Frank Herbert',
      genre: 'Sci-Fi',
      publicationYear: 1965,
      readingStatus: 'Read',
      rating: 5,
      notes: null
    };

    service.create(request).subscribe((book) => expect(book).toEqual(sampleBook));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(sampleBook);
  });

  it('update sends a PUT to /books/{id}', () => {
    const request: BookRequest = { ...sampleBook, title: 'Dune Messiah' };

    service.update('1', request).subscribe();

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('PUT');
    req.flush({ ...sampleBook, title: 'Dune Messiah' });
  });

  it('delete sends a DELETE to /books/{id}', () => {
    service.delete('1').subscribe();

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});

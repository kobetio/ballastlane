import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { Book } from '../../../models/book.model';
import { BookDetail } from './book-detail';

describe('BookDetail', () => {
  let component: BookDetail;
  let fixture: ComponentFixture<BookDetail>;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/books`;

  const book: Book = {
    id: '42',
    title: 'Dune',
    author: 'Frank Herbert',
    genre: 'Sci-Fi',
    publicationYear: 1965,
    readingStatus: 'Read',
    rating: 5,
    notes: 'Great book.'
  };

  async function configure(paramId: string | null) {
    await TestBed.configureTestingModule({
      imports: [BookDetail],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideNoopAnimations(),
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap(paramId ? { id: paramId } : {}) } }
        }
      ]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(BookDetail);
    component = fixture.componentInstance;
  }

  afterEach(() => httpMock.verify());

  it('loads the book by id from the route and exposes it as a signal', async () => {
    await configure('42');

    httpMock.expectOne(`${baseUrl}/42`).flush(book);

    expect(component.isLoading()).toBe(false);
    expect(component.book()).toEqual(book);
    expect(component.readingStatusLabel()).toBe('Read');
    expect(component.ratingStars()).toBe('★★★★★');
  });

  it('sets an error when no id is present in the route', async () => {
    await configure(null);

    expect(component.isLoading()).toBe(false);
    expect(component.loadError()).toBeTruthy();
  });

  it('sets a friendly error message when the request fails', async () => {
    await configure('42');

    httpMock.expectOne(`${baseUrl}/42`).flush('not found', { status: 404, statusText: 'Not Found' });

    expect(component.loadError()).toBeTruthy();
  });
});

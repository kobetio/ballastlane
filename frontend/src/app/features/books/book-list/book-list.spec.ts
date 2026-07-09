import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { environment } from '../../../../environments/environment';
import { Book } from '../../../models/book.model';
import { BookList } from './book-list';

describe('BookList', () => {
  let component: BookList;
  let fixture: ComponentFixture<BookList>;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.apiBaseUrl}/books`;

  const books: Book[] = [
    { id: '1', title: 'The Pragmatic Programmer', author: 'Andrew Hunt', genre: null, publicationYear: null, readingStatus: null, rating: null, notes: null },
    { id: '2', title: 'Clean Code', author: 'Robert C. Martin', genre: null, publicationYear: null, readingStatus: null, rating: null, notes: null }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookList],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), provideNoopAnimations()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(BookList);
    component = fixture.componentInstance;
  });

  afterEach(() => httpMock.verify());

  it('starts in a loading state and requests the book list', () => {
    expect(component.isLoading()).toBe(true);
    httpMock.expectOne(baseUrl).flush(books);
  });

  it('sorts the loaded books by title and clears the loading state', () => {
    httpMock.expectOne(baseUrl).flush(books);

    expect(component.isLoading()).toBe(false);
    expect(component.sortedBooks().map((b) => b.title)).toEqual(['Clean Code', 'The Pragmatic Programmer']);
  });

  it('reports the empty state when there are no books', () => {
    httpMock.expectOne(baseUrl).flush([]);

    expect(component.isEmpty()).toBe(true);
  });

  it('reports a friendly error message when loading fails', () => {
    httpMock.expectOne(baseUrl).flush('boom', { status: 500, statusText: 'Server Error' });

    expect(component.isLoading()).toBe(false);
    expect(component.loadError()).toBeTruthy();
  });

  it('navigates to the book details page on viewDetails', () => {
    httpMock.expectOne(baseUrl).flush(books);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    component.viewDetails(books[0]);

    expect(navigateSpy).toHaveBeenCalledWith(['/books', '1']);
  });
});

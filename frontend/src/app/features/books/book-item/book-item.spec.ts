import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Book } from '../../../models/book.model';
import { BookItem } from './book-item';

describe('BookItem', () => {
  let component: BookItem;
  let fixture: ComponentFixture<BookItem>;

  const book: Book = {
    id: '1',
    title: 'Dune',
    author: 'Frank Herbert',
    genre: 'Sci-Fi',
    publicationYear: 1965,
    readingStatus: 'Read',
    rating: 4,
    notes: null
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [BookItem] }).compileComponents();
    fixture = TestBed.createComponent(BookItem);
    fixture.componentRef.setInput('book', book);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('maps the reading status to its friendly label', () => {
    expect(component.readingStatusLabel()).toBe('Read');
  });

  it('renders the rating as filled/empty stars', () => {
    expect(component.ratingStars()).toBe('★★★★☆');
  });

  it('emits view/edit/delete when the corresponding action fires', () => {
    let viewed = false;
    let edited = false;
    let deleted = false;
    component.view.subscribe(() => (viewed = true));
    component.edit.subscribe(() => (edited = true));
    component.delete.subscribe(() => (deleted = true));

    component.view.emit();
    component.edit.emit();
    component.delete.emit();

    expect(viewed).toBe(true);
    expect(edited).toBe(true);
    expect(deleted).toBe(true);
  });
});

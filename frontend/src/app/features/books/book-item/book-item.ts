import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { Book, READING_STATUS_OPTIONS } from '../../../models/book.model';

/** Presentational card for a single book; the parent (`BookList`) owns all behavior. */
@Component({
  selector: 'app-book-item',
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  templateUrl: './book-item.html',
  styleUrl: './book-item.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BookItem {
  readonly book = input.required<Book>();

  readonly view = output<void>();
  readonly edit = output<void>();
  readonly delete = output<void>();

  readonly readingStatusLabel = computed(() => {
    const status = this.book().readingStatus;
    return READING_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? null;
  });

  readonly ratingStars = computed(() => {
    const rating = this.book().rating;
    return rating ? '★'.repeat(rating) + '☆'.repeat(5 - rating) : null;
  });
}

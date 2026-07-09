import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router } from '@angular/router';
import { BookService } from '../../../core/services/book.service';
import { Book, READING_STATUS_OPTIONS } from '../../../models/book.model';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { getApiErrorMessage } from '../../../shared/utils/api-error.util';
import { BookForm, BookFormData } from '../book-form/book-form';

/** Full-page details view for a single book, with Edit/Delete actions and a back link. */
@Component({
  selector: 'app-book-detail',
  imports: [MatCardModule, MatButtonModule, MatIconModule, MatChipsModule, MatProgressSpinnerModule],
  templateUrl: './book-detail.html',
  styleUrl: './book-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BookDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly bookService = inject(BookService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly book = signal<Book | null>(null);
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);

  readonly readingStatusLabel = computed(() => {
    const status = this.book()?.readingStatus;
    return READING_STATUS_OPTIONS.find((option) => option.value === status)?.label ?? null;
  });

  readonly ratingStars = computed(() => {
    const rating = this.book()?.rating;
    return rating ? '★'.repeat(rating) + '☆'.repeat(5 - rating) : null;
  });

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadBook(id);
    } else {
      this.loadError.set('No book id was provided.');
      this.isLoading.set(false);
    }
  }

  private loadBook(id: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.bookService.getById(id).subscribe({
      next: (book) => {
        this.book.set(book);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.loadError.set(getApiErrorMessage(error, 'Could not load this book.'));
      }
    });
  }

  goBack(): void {
    this.router.navigateByUrl('/books');
  }

  edit(): void {
    const book = this.book();
    if (!book) {
      return;
    }

    const dialogRef = this.dialog.open<BookForm, BookFormData, Book | undefined>(BookForm, {
      data: { book },
      width: '480px'
    });

    dialogRef.afterClosed().subscribe((updatedBook) => {
      if (updatedBook) {
        this.book.set(updatedBook);
        this.snackBar.open(`"${updatedBook.title}" was updated.`, 'Dismiss', { duration: 4000 });
      }
    });
  }

  delete(): void {
    const book = this.book();
    if (!book) {
      return;
    }

    const data: ConfirmDialogData = {
      title: 'Delete book',
      message: `Are you sure you want to delete "${book.title}"? This cannot be undone.`,
      confirmLabel: 'Delete'
    };

    const dialogRef = this.dialog.open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data, width: '400px' });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.bookService.delete(book.id).subscribe({
          next: () => {
            this.snackBar.open(`"${book.title}" was deleted.`, 'Dismiss', { duration: 4000 });
            this.router.navigateByUrl('/books');
          },
          error: (error: unknown) => {
            this.snackBar.open(getApiErrorMessage(error, 'Could not delete this book.'), 'Dismiss', { duration: 5000 });
          }
        });
      }
    });
  }
}

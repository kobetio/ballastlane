import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router } from '@angular/router';
import { BookService } from '../../../core/services/book.service';
import { Book } from '../../../models/book.model';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { getApiErrorMessage } from '../../../shared/utils/api-error.util';
import { BookForm, BookFormData } from '../book-form/book-form';
import { BookItem } from '../book-item/book-item';

/**
 * Smart component owning the Books list: loads the current user's books, exposes
 * loading/empty/error states, and orchestrates the create/edit dialog, the delete
 * confirmation dialog, and navigation to the details page.
 */
@Component({
  selector: 'app-book-list',
  imports: [BookItem, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatToolbarModule],
  templateUrl: './book-list.html',
  styleUrl: './book-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BookList {
  private readonly bookService = inject(BookService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);

  private readonly books = signal<Book[]>([]);
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);

  readonly sortedBooks = computed(() =>
    [...this.books()].sort((a, b) => a.title.localeCompare(b.title))
  );
  readonly isEmpty = computed(() => !this.isLoading() && !this.loadError() && this.books().length === 0);

  constructor() {
    this.loadBooks();
  }

  loadBooks(): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.bookService.getAll().subscribe({
      next: (books) => {
        this.books.set(books);
        this.isLoading.set(false);
      },
      error: (error: unknown) => {
        this.isLoading.set(false);
        this.loadError.set(getApiErrorMessage(error, 'Could not load your books. Please try again.'));
      }
    });
  }

  viewDetails(book: Book): void {
    this.router.navigate(['/books', book.id]);
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open<BookForm, BookFormData, Book | undefined>(BookForm, {
      data: { book: null },
      width: '480px'
    });

    dialogRef.afterClosed().subscribe((createdBook) => {
      if (createdBook) {
        this.books.update((current) => [...current, createdBook]);
        this.snackBar.open(`"${createdBook.title}" was added to your library.`, 'Dismiss', { duration: 4000 });
      }
    });
  }

  openEditDialog(book: Book): void {
    const dialogRef = this.dialog.open<BookForm, BookFormData, Book | undefined>(BookForm, {
      data: { book },
      width: '480px'
    });

    dialogRef.afterClosed().subscribe((updatedBook) => {
      if (updatedBook) {
        this.books.update((current) => current.map((b) => (b.id === updatedBook.id ? updatedBook : b)));
        this.snackBar.open(`"${updatedBook.title}" was updated.`, 'Dismiss', { duration: 4000 });
      }
    });
  }

  openDeleteConfirm(book: Book): void {
    const data: ConfirmDialogData = {
      title: 'Delete book',
      message: `Are you sure you want to delete "${book.title}"? This cannot be undone.`,
      confirmLabel: 'Delete'
    };

    const dialogRef = this.dialog.open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, { data, width: '400px' });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.deleteBook(book);
      }
    });
  }

  private deleteBook(book: Book): void {
    this.bookService.delete(book.id).subscribe({
      next: () => {
        this.books.update((current) => current.filter((b) => b.id !== book.id));
        this.snackBar.open(`"${book.title}" was deleted.`, 'Dismiss', { duration: 4000 });
      },
      error: (error: unknown) => {
        this.snackBar.open(getApiErrorMessage(error, 'Could not delete this book. Please try again.'), 'Dismiss', {
          duration: 5000
        });
      }
    });
  }
}

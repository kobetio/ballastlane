import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BookService } from '../../../core/services/book.service';
import { Book, READING_STATUS_OPTIONS, ReadingStatus } from '../../../models/book.model';
import { applyServerValidationErrors, getApiErrorMessage } from '../../../shared/utils/api-error.util';

export interface BookFormData {
  book: Book | null;
}

const CURRENT_YEAR = new Date().getFullYear();

/**
 * Create/Edit dialog for a book. Performs the actual `POST`/`PUT` call itself (like the Login
 * and Register components do) so it can map field-level server validation errors back onto the
 * relevant control and keep the dialog open for the user to fix them, only closing (with the
 * saved book) once the request actually succeeds.
 */
@Component({
  selector: 'app-book-form',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './book-form.html',
  styleUrl: './book-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BookForm {
  private readonly formBuilder = inject(FormBuilder);
  private readonly bookService = inject(BookService);
  private readonly dialogRef = inject(MatDialogRef<BookForm>);
  private readonly snackBar = inject(MatSnackBar);
  readonly data = inject<BookFormData>(MAT_DIALOG_DATA);

  readonly isSubmitting = signal(false);
  readonly currentYear = CURRENT_YEAR;
  readonly readingStatusOptions = READING_STATUS_OPTIONS;

  readonly isEditMode = this.data.book !== null;

  readonly form = this.formBuilder.nonNullable.group({
    title: [this.data.book?.title ?? '', [Validators.required, Validators.maxLength(150)]],
    author: [this.data.book?.author ?? '', [Validators.required, Validators.maxLength(100)]],
    genre: [this.data.book?.genre ?? '', [Validators.maxLength(50)]],
    publicationYear: [this.data.book?.publicationYear ?? null as number | null, [Validators.min(1450), Validators.max(CURRENT_YEAR)]],
    readingStatus: [this.data.book?.readingStatus ?? (null as ReadingStatus | null)],
    rating: [this.data.book?.rating ?? null as number | null, [Validators.min(1), Validators.max(5)]],
    notes: [this.data.book?.notes ?? '', [Validators.maxLength(1000)]]
  });

  get titleControl() {
    return this.form.controls.title;
  }

  get authorControl() {
    return this.form.controls.author;
  }

  get genreControl() {
    return this.form.controls.genre;
  }

  get publicationYearControl() {
    return this.form.controls.publicationYear;
  }

  get ratingControl() {
    return this.form.controls.rating;
  }

  get notesControl() {
    return this.form.controls.notes;
  }

  cancel(): void {
    this.dialogRef.close();
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();
    const request = {
      title: raw.title,
      author: raw.author,
      genre: raw.genre || null,
      publicationYear: raw.publicationYear,
      readingStatus: raw.readingStatus,
      rating: raw.rating,
      notes: raw.notes || null
    };

    const request$ = this.isEditMode
      ? this.bookService.update(this.data.book!.id, request)
      : this.bookService.create(request);

    request$.subscribe({
      next: (book) => this.dialogRef.close(book),
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        if (error.status === 400) {
          applyServerValidationErrors(this.form, error.error?.errors);
        } else {
          this.snackBar.open(getApiErrorMessage(error), 'Dismiss', { duration: 5000 });
        }
      }
    });
  }
}

import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';
import { ApiProblemDetails } from '../../models/api-error.model';

/**
 * Extracts a human-readable summary message from a failed HTTP request, preferring the
 * API's own wording (Specification.md §6/§8: "Display API error messages exactly as returned")
 * over any generic client-side text.
 */
export function getApiErrorMessage(error: unknown, fallback = 'An unexpected error occurred. Please try again.'): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ApiProblemDetails | null;
    if (problem?.detail) {
      return problem.detail;
    }
    if (problem?.errors) {
      return Object.values(problem.errors).flat().join(' ');
    }
    if (problem?.title) {
      return problem.title;
    }
  }
  return fallback;
}

/**
 * Maps a `ValidationProblemDetails.errors` dictionary (keyed by PascalCase property name,
 * e.g. `"Email"`) onto the matching reactive form controls (keyed by camelCase, e.g. `"email"`),
 * so each field displays the exact message the API returned instead of a generic one.
 */
export function applyServerValidationErrors(form: FormGroup, errors: Record<string, string[]> | undefined): void {
  if (!errors) {
    return;
  }

  for (const [field, messages] of Object.entries(errors)) {
    const controlName = field.charAt(0).toLowerCase() + field.slice(1);
    const control = form.get(controlName);
    if (control) {
      control.setErrors({ ...control.errors, server: messages.join(' ') });
    }
  }
}

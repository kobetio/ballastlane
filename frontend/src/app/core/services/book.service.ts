import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Book, BookRequest } from '../../models/book.model';

/** Thin HTTP client for the Books CRUD endpoints; all business rules live server-side. */
@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/books`;

  getAll(): Observable<Book[]> {
    return this.http.get<Book[]>(this.baseUrl);
  }

  getById(id: string): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(request: BookRequest): Observable<Book> {
    return this.http.post<Book>(this.baseUrl, request);
  }

  update(id: string, request: BookRequest): Observable<Book> {
    return this.http.put<Book>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

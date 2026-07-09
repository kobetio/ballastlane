import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
    canActivate: [guestGuard]
  },
  {
    path: '',
    loadComponent: () => import('./layout/main-layout/main-layout').then((m) => m.MainLayout),
    canActivate: [authGuard],
    children: [
      {
        path: 'books',
        loadComponent: () => import('./features/books/book-list/book-list').then((m) => m.BookList)
      },
      {
        path: 'books/:id',
        loadComponent: () => import('./features/books/book-detail/book-detail').then((m) => m.BookDetail)
      },
      { path: '', pathMatch: 'full', redirectTo: 'books' }
    ]
  },
  { path: '**', redirectTo: '' }
];

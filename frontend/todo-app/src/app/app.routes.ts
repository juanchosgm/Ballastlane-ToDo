import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then((m) => m.Login),
    title: 'To-Do • Sign in',
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/todos/todo-list/todo-list').then((m) => m.TodoList),
    title: 'To-Do • Tasks',
  },
  {
    path: 'todos/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/todos/todo-detail/todo-detail').then((m) => m.TodoDetail),
    title: 'To-Do • Details',
  },
  { path: '**', redirectTo: '' },
];

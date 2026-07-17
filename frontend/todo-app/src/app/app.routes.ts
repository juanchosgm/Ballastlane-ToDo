import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/todos/todo-list/todo-list').then((m) => m.TodoList),
    title: 'To-Do • Tasks',
  },
  {
    path: 'todos/:id',
    loadComponent: () =>
      import('./features/todos/todo-detail/todo-detail').then((m) => m.TodoDetail),
    title: 'To-Do • Details',
  },
  { path: '**', redirectTo: '' },
];

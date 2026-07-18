import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TodoService } from '../../../core/services/todo.service';
import { STATUS_META, TodoStatus, TodoSummary } from '../../../core/models/todo.model';
import { TodoFormDialog, TodoFormDialogData } from '../todo-form-dialog/todo-form-dialog';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-todo-list',
  imports: [
    DatePipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
  ],
  templateUrl: './todo-list.html',
  styleUrl: './todo-list.scss',
})
export class TodoList implements OnInit {
  private readonly service = inject(TodoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly todos = signal<TodoSummary[]>([]);
  readonly loading = signal(true);
  readonly error = signal(false);

  readonly statusMeta = STATUS_META;

  readonly total = computed(() => this.todos().length);
  readonly pending = computed(() => this.todos().filter((t) => t.status === 'Pending').length);
  readonly inProgress = computed(() => this.todos().filter((t) => t.status === 'InProgress').length);
  readonly done = computed(() => this.todos().filter((t) => t.status === 'Done').length);

  isDone(todo: TodoSummary): boolean {
    return todo.status === 'Done';
  }

  /** A task is overdue when its due date has passed and it is not finished yet. */
  isOverdue(todo: TodoSummary): boolean {
    return todo.status !== 'Done' && todo.dueDate != null && new Date(todo.dueDate) < new Date();
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(false);
    this.service.getAll().subscribe({
      next: (items) => {
        this.todos.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      },
    });
  }

  openCreate(): void {
    this.dialog
      .open(TodoFormDialog, { data: { mode: 'create' } satisfies TodoFormDialogData })
      .afterClosed()
      .subscribe((created) => created && this.load());
  }

  openEdit(todo: TodoSummary): void {
    // The edit form needs the full detail (description), which the list omits.
    this.service.getById(todo.id).subscribe({
      next: (detail) => {
        this.dialog
          .open(TodoFormDialog, { data: { mode: 'edit', todo: detail } satisfies TodoFormDialogData })
          .afterClosed()
          .subscribe((updated) => updated && this.load());
      },
      error: () => this.snackBar.open('Could not load the task.', 'Dismiss', { duration: 3000 }),
    });
  }

  toggleComplete(todo: TodoSummary): void {
    // Preserve title/description/due date; only flip between Done and Pending.
    this.service.getById(todo.id).subscribe({
      next: (detail) => {
        const nextStatus: TodoStatus = detail.status === 'Done' ? 'Pending' : 'Done';
        this.service
          .update(todo.id, {
            title: detail.title,
            description: detail.description,
            status: nextStatus,
            dueDate: detail.dueDate,
          })
          .subscribe({
            next: () => this.load(),
            error: () => this.snackBar.open('Could not update the task.', 'Dismiss', { duration: 3000 }),
          });
      },
    });
  }

  confirmDelete(todo: TodoSummary): void {
    const data: ConfirmDialogData = {
      title: 'Delete task',
      message: `Delete "${todo.title}"? This cannot be undone.`,
      confirmText: 'Delete',
    };
    this.dialog
      .open(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(todo.id).subscribe({
          next: () => {
            this.snackBar.open('Task deleted', 'Dismiss', { duration: 2500 });
            this.load();
          },
          error: () => this.snackBar.open('Could not delete the task.', 'Dismiss', { duration: 3000 }),
        });
      });
  }
}

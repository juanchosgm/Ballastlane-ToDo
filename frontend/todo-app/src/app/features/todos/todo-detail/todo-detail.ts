import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TodoService } from '../../../core/services/todo.service';
import { STATUS_META, TodoDetail as TodoDetailModel, TodoStatus } from '../../../core/models/todo.model';
import { TodoFormDialog, TodoFormDialogData } from '../todo-form-dialog/todo-form-dialog';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-todo-detail',
  imports: [
    DatePipe,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatDividerModule,
  ],
  templateUrl: './todo-detail.html',
  styleUrl: './todo-detail.scss',
})
export class TodoDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly service = inject(TodoService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly todo = signal<TodoDetailModel | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);
  readonly statusMeta = STATUS_META;

  isOverdue(todo: TodoDetailModel): boolean {
    return todo.status !== 'Done' && todo.dueDate != null && new Date(todo.dueDate) < new Date();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) this.load(id);
  }

  load(id: string): void {
    this.loading.set(true);
    this.notFound.set(false);
    this.service.getById(id).subscribe({
      next: (detail) => {
        this.todo.set(detail);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }

  toggleComplete(): void {
    const current = this.todo();
    if (!current) return;
    const nextStatus: TodoStatus = current.status === 'Done' ? 'Pending' : 'Done';
    this.service
      .update(current.id, {
        title: current.title,
        description: current.description,
        status: nextStatus,
        dueDate: current.dueDate,
      })
      .subscribe({
        next: (updated) => this.todo.set(updated),
        error: () => this.snackBar.open('Could not update the task.', 'Dismiss', { duration: 3000 }),
      });
  }

  edit(): void {
    const current = this.todo();
    if (!current) return;
    this.dialog
      .open(TodoFormDialog, { data: { mode: 'edit', todo: current } satisfies TodoFormDialogData })
      .afterClosed()
      .subscribe((updated: TodoDetailModel | undefined) => updated && this.todo.set(updated));
  }

  confirmDelete(): void {
    const current = this.todo();
    if (!current) return;
    const data: ConfirmDialogData = {
      title: 'Delete task',
      message: `Delete "${current.title}"? This cannot be undone.`,
      confirmText: 'Delete',
    };
    this.dialog
      .open(ConfirmDialog, { data })
      .afterClosed()
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.service.delete(current.id).subscribe({
          next: () => {
            this.snackBar.open('Task deleted', 'Dismiss', { duration: 2500 });
            this.router.navigate(['/']);
          },
          error: () => this.snackBar.open('Could not delete the task.', 'Dismiss', { duration: 3000 }),
        });
      });
  }
}

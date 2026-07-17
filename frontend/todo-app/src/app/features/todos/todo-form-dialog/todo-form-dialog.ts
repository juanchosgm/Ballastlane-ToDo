import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';

import { TodoService } from '../../../core/services/todo.service';
import { TodoDetail } from '../../../core/models/todo.model';

export interface TodoFormDialogData {
  mode: 'create' | 'edit';
  todo?: TodoDetail;
}

@Component({
  selector: 'app-todo-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressBarModule,
  ],
  templateUrl: './todo-form-dialog.html',
  styleUrl: './todo-form-dialog.scss',
})
export class TodoFormDialog {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(TodoService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialogRef = inject(MatDialogRef<TodoFormDialog, TodoDetail>);
  readonly data = inject<TodoFormDialogData>(MAT_DIALOG_DATA);

  readonly isEdit = this.data.mode === 'edit';
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    title: [
      this.data.todo?.title ?? '',
      [Validators.required, Validators.maxLength(200)],
    ],
    description: [
      this.data.todo?.description ?? '',
      [Validators.maxLength(2000)],
    ],
    isCompleted: [this.data.todo?.isCompleted ?? false],
  });

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const description = value.description.trim() === '' ? null : value.description.trim();

    const request$ = this.isEdit
      ? this.service.update(this.data.todo!.id, {
          title: value.title.trim(),
          description,
          isCompleted: value.isCompleted,
        })
      : this.service.create({ title: value.title.trim(), description });

    request$.subscribe({
      next: (result) => {
        this.snackBar.open(
          this.isEdit ? 'Task updated' : 'Task created',
          'Dismiss',
          { duration: 2500 },
        );
        this.dialogRef.close(result);
      },
      error: () => {
        this.saving.set(false);
        this.snackBar.open('Something went wrong. Please try again.', 'Dismiss', {
          duration: 3500,
        });
      },
    });
  }
}

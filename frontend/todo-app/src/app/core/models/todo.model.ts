/** Lifecycle state of a task. Mirrors the backend `TodoStatus` enum (serialised by name). */
export type TodoStatus = 'Pending' | 'InProgress' | 'Done';

/** UI metadata for each status: the human label, an icon and a css modifier class. */
export const STATUS_META: Record<TodoStatus, { label: string; icon: string; cls: string }> = {
  Pending: { label: 'Pending', icon: 'radio_button_unchecked', cls: 'pending' },
  InProgress: { label: 'In progress', icon: 'timelapse', cls: 'in-progress' },
  Done: { label: 'Done', icon: 'check_circle', cls: 'done' },
};

/** Ordered list of statuses, handy for building <select> options. */
export const STATUS_OPTIONS: TodoStatus[] = ['Pending', 'InProgress', 'Done'];

/** Row shape returned by the list endpoint (no description by design). */
export interface TodoSummary {
  id: string;
  title: string;
  status: TodoStatus;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string | null;
}

/** Full shape returned by the details endpoint (includes description). */
export interface TodoDetail extends TodoSummary {
  description: string | null;
}

export interface CreateTodoRequest {
  title: string;
  description: string | null;
  dueDate: string | null;
}

export interface UpdateTodoRequest {
  title: string;
  description: string | null;
  status: TodoStatus;
  dueDate: string | null;
}

/** Row shape returned by the list endpoint (no description by design). */
export interface TodoSummary {
  id: string;
  title: string;
  isCompleted: boolean;
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
}

export interface UpdateTodoRequest {
  title: string;
  description: string | null;
  isCompleted: boolean;
}

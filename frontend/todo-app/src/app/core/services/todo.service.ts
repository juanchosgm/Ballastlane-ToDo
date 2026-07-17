import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateTodoRequest,
  TodoDetail,
  TodoSummary,
  UpdateTodoRequest,
} from '../models/todo.model';

/** Thin HTTP gateway to the To-Do REST API. */
@Injectable({ providedIn: 'root' })
export class TodoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/todos`;

  getAll(): Observable<TodoSummary[]> {
    return this.http.get<TodoSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<TodoDetail> {
    return this.http.get<TodoDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateTodoRequest): Observable<TodoDetail> {
    return this.http.post<TodoDetail>(this.baseUrl, request);
  }

  update(id: string, request: UpdateTodoRequest): Observable<TodoDetail> {
    return this.http.put<TodoDetail>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}

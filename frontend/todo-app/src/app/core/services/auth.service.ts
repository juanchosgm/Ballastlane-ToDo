import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AccessTokenResponse, LoginRequest } from '../models/auth.model';

const TOKEN_KEY = 'todo.accessToken';
const EMAIL_KEY = 'todo.userEmail';

/** Owns the authentication state (bearer token) and talks to the Identity endpoints. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  // Signals are seeded from storage so a page refresh keeps the session.
  private readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly email = signal<string | null>(localStorage.getItem(EMAIL_KEY));

  readonly accessToken = this.token.asReadonly();
  readonly userEmail = this.email.asReadonly();
  readonly isAuthenticated = computed(() => this.token() !== null);

  login(credentials: LoginRequest): Observable<AccessTokenResponse> {
    return this.http
      .post<AccessTokenResponse>(`${this.baseUrl}/login`, credentials)
      .pipe(tap((response) => this.setSession(response.accessToken, credentials.email)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    this.token.set(null);
    this.email.set(null);
  }

  private setSession(accessToken: string, email: string): void {
    localStorage.setItem(TOKEN_KEY, accessToken);
    localStorage.setItem(EMAIL_KEY, email);
    this.token.set(accessToken);
    this.email.set(email);
  }
}

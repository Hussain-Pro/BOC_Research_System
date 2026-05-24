import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private token: string | null = null;

  login(email: string, password: string): Observable<any> {
    // Replace with actual API call
    if (email && password.length >= 6) {
      this.token = 'mock-jwt-token';
      localStorage.setItem('boc_token', this.token);
      return of({ success: true, token: this.token }).pipe(delay(800));
    }
    return throwError(() => new Error('AUTH.INVALID_CREDENTIALS')).pipe(delay(500));
  }

  logout(): void {
    this.token = null;
    localStorage.removeItem('boc_token');
    window.location.href = '/login';
  }

  isAuthenticated(): boolean {
    return !!this.token || !!localStorage.getItem('boc_token');
  }

  getToken(): string | null {
    return this.token || localStorage.getItem('boc_token');
  }
}

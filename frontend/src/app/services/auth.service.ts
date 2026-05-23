import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private jwtHelper = new JwtHelperService();
  
  private apiUrl = 'https://localhost:7139/api/Auth';

  constructor() { }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, credentials).pipe(
      tap((response: any) => {
        if (response.token) {
          localStorage.setItem('boc_token', response.token);
          localStorage.setItem('boc_refresh', response.refreshToken);
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('boc_token');
    localStorage.removeItem('boc_refresh');
    this.router.navigate(['/auth/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('boc_token');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    return !this.jwtHelper.isTokenExpired(token);
  }

  // Backwards compatibility for existing components
  token(): string | null {
    return this.getToken();
  }

  currentUser(): any {
    const token = this.getToken();
    if (token) {
      return this.jwtHelper.decodeToken(token);
    }
    return { name: 'المستخدم الحالي', role: 'System Administrator' };
  }

  refreshToken(): Observable<any> {
    const token = this.getToken();
    const refreshToken = localStorage.getItem('boc_refresh');
    return this.http.post(`${this.apiUrl}/refresh-token`, { token, refreshToken }).pipe(
      tap((response: any) => {
        localStorage.setItem('boc_token', response.token);
        localStorage.setItem('boc_refresh', response.refreshToken);
      })
    );
  }

  register(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  verify2Fa(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/verify-2fa`, data).pipe(
      tap((response: any) => {
        if (response.accessToken) {
          localStorage.setItem('boc_token', response.accessToken);
          localStorage.setItem('boc_refresh', response.refreshToken);
        }
      })
    );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }
}

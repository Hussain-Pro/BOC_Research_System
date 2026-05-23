import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('auth/login')) {
        // Handle 401 Unauthorized globally - trigger refresh token or logout
        return authService.refreshToken().pipe(
          switchMap(() => {
            // If successful, retry the request
            const cloned = req.clone({
              setHeaders: {
                Authorization: `Bearer ${authService.getToken()}`
              }
            });
            return next(cloned);
          }),
          catchError((err) => {
            // If refresh fails, force logout
            authService.logout();
            router.navigate(['/auth/login']);
            return throwError(() => err);
          })
        );
      }
      
      const err = error.error?.message || error.statusText;
      console.error('API Error:', err);
      return throwError(() => error);
    })
  );
};

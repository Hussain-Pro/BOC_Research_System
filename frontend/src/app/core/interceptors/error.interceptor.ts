import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

// Translate common English API error messages to Arabic
function translateErrorMessage(msg: string): string {
  if (!msg) return '';
  const translations: { [key: string]: string } = {
    'Invalid email or password.': 'البريد الإلكتروني أو كلمة المرور غير صحيحة.',
    'Your account is pending HR verification and cannot log in yet.': 'حسابك قيد مراجعة الموارد البشرية ولا يمكن تسجيل الدخول بعد.',
    'Your account has been locked. Please contact administration.': 'تم تأمين حسابك. يرجى التواصل مع الإدارة.',
    'User not found.': 'المستخدم غير موجود.',
    '2FA has not been initialized for this account.': 'لم يتم إعداد المصادقة الثنائية لهذا الحساب.',
    'Invalid two-factor authentication code.': 'رمز المصادقة الثنائية غير صحيح.',
    'Invalid or expired reset token.': 'رمز إعادة التعيين غير صالح أو منتهي الصلاحية.',
    'Email is required.': 'البريد الإلكتروني مطلوب.',
    'A valid email address is required.': 'يرجى إدخال بريد إلكتروني صالح.',
    'Password is required.': 'كلمة المرور مطلوبة.',
    'Code is required.': 'الرمز مطلوب.',
    'TOTP code must be 6 digits.': 'يجب أن يكون رمز التحقق مكوناً من 6 أرقام.',
    'One or more validation errors occurred.': 'حدثت أخطاء في التحقق من البيانات.',
  };
  return translations[msg] || msg;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const toastService = inject(ToastService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Network / CORS / Connection Refused
      if (error.status === 0) {
        toastService.error('تعذّر الاتصال بالخادم. يرجى التحقق من تشغيل الخادم والمحاولة مجدداً.');
        return throwError(() => error);
      }

      if (error.status === 401 && !req.url.includes('auth/login')) {
        return authService.refreshToken().pipe(
          switchMap(() => {
            const cloned = req.clone({
              setHeaders: { Authorization: `Bearer ${authService.getToken()}` }
            });
            return next(cloned);
          }),
          catchError((err) => {
            authService.logout();
            toastService.warning('انتهت صلاحية جلستك. يرجى تسجيل الدخول مجدداً.');
            router.navigate(['/auth/login']);
            return throwError(() => err);
          })
        );
      }

      // Extract the error message from the response
      const apiErrors: string[] = error.error?.errors ?? [];
      const apiMessage: string = error.error?.detail || error.error?.title || error.error?.message || '';
      
      // Don't show global toast for auth-page errors (they handle it themselves)
      const isAuthRequest = req.url.includes('/api/Auth/');
      
      if (!isAuthRequest) {
        let displayMessage = '';
        if (apiErrors.length > 0) {
          displayMessage = apiErrors.map(translateErrorMessage).join(' — ');
        } else if (apiMessage) {
          displayMessage = translateErrorMessage(apiMessage);
        }

        if (!displayMessage) {
          if (error.status === 400) displayMessage = 'طلب غير صالح. يرجى مراجعة البيانات المدخلة.';
          else if (error.status === 403) displayMessage = 'ليس لديك صلاحية للوصول إلى هذا المورد.';
          else if (error.status === 404) displayMessage = 'المورد المطلوب غير موجود.';
          else if (error.status === 422) displayMessage = 'البيانات المدخلة غير مكتملة أو غير صحيحة.';
          else if (error.status === 429) displayMessage = 'لقد تجاوزت الحد المسموح به من الطلبات. يرجى الانتظار قليلاً.';
          else if (error.status >= 500) displayMessage = 'حدث خطأ في الخادم. يرجى المحاولة لاحقاً أو التواصل مع الدعم التقني.';
          else displayMessage = 'حدث خطأ غير متوقع.';
        }

        toastService.error(displayMessage);
      }

      console.error('API Error:', error);
      return throwError(() => error);
    })
  );
};


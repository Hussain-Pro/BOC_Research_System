import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { BocAuthShellComponent } from '../../shared/boc-auth-shell/boc-auth-shell.component';
import { BocFormFieldComponent } from '../../shared/boc-form-field/boc-form-field.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BocAuthShellComponent, BocFormFieldComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  isLoading = false;
  errorMessage = '';

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  onSubmit() {
    if (this.loginForm.invalid) return;
    this.isLoading = true;
    this.errorMessage = '';

    const credentials = this.loginForm.getRawValue();
    const deviceFingerprint = btoa(navigator.userAgent + navigator.language + screen.colorDepth).substring(0, 32);

    this.authService.login({ ...credentials, deviceFingerprint }).subscribe({
      next: (res) => {
        if (res.requiresTwoFactorVerification || res.requiresTwoFactorSetup) {
          this.router.navigate(['/auth/2fa'], {
            state: {
              email: credentials.email,
              requiresSetup: res.requiresTwoFactorSetup,
              qrCodeUrl: res.twoFactorQrCodeUrl,
              secret: res.twoFactorSecret
            }
          });
        } else {
          this.toastService.success('تم تسجيل الدخول بنجاح.');
          const role = this.authService.getRole();
          this.router.navigate([role === 'Admin' ? '/admin/analytics' : '/home']);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'فشل تسجيل الدخول. يرجى التحقق من البيانات.';
        this.toastService.error(this.errorMessage);
      }
    });
  }
}

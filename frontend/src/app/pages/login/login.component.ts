import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  credentials = { email: '', password: '' };
  isLoading = false;
  errorMessage = '';
  
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  onSubmit() {
    this.isLoading = true;
    
    // Generate a simple device fingerprint (in production, use a more robust solution)
    const deviceFingerprint = btoa(navigator.userAgent + navigator.language + screen.colorDepth).substring(0, 32);
    
    const payload = {
      ...this.credentials,
      deviceFingerprint
    };

    this.authService.login(payload).subscribe({
      next: (res) => {
        if (res.requiresTwoFactorVerification || res.requiresTwoFactorSetup) {
          // Pass the email and response data to the 2FA component via state or query params
          const stateData = { 
            email: this.credentials.email,
            requiresSetup: res.requiresTwoFactorSetup,
            qrCodeUrl: res.twoFactorQrCodeUrl,
            secret: res.twoFactorSecret
          };
          this.router.navigate(['/auth/2fa'], { state: stateData });
        } else {
          this.toastService.success('تم تسجيل الدخول بنجاح.');
          const role = this.authService.getRole();
          if (role === 'Admin') {
            this.router.navigate(['/admin/analytics']);
          } else {
            this.router.navigate(['/home']);
          }
        }
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        this.toastService.error(err.error?.detail || err.error?.title || 'فشل تسجيل الدخول. يرجى التحقق من البيانات.');
      }
    });
  }
}


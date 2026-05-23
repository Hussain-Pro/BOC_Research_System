import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

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
  
  private authService = inject(AuthService);
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
          this.router.navigate(['/research/timeline']);
        }
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert(err.error?.title || err.error?.detail || 'فشل تسجيل الدخول. يرجى التحقق من البيانات.');
      }
    });
  }
}

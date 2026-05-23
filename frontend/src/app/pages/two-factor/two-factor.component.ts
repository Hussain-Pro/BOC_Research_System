import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-two-factor',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './two-factor.component.html',
  styleUrl: './two-factor.component.scss'
})
export class TwoFactorComponent implements OnInit {
  code: string = '';
  email: string = '';
  
  requiresSetup: boolean = false;
  qrCodeUrl: string = '';
  secret: string = '';
  
  isLoading = false;
  
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    // Retrieve state from router
    const state = history.state;
    if (state && state.email) {
      this.email = state.email;
      this.requiresSetup = state.requiresSetup || false;
      this.qrCodeUrl = state.qrCodeUrl || '';
      this.secret = state.secret || '';
    } else {
      // If accessed directly without state, redirect to login
      this.router.navigate(['/auth/login']);
    }
  }

  onSubmit() {
    if (!this.code || this.code.length < 6) return;
    
    this.isLoading = true;
    
    const payload = {
      email: this.email,
      twoFactorCode: this.code,
      deviceFingerprint: btoa(navigator.userAgent + navigator.language + screen.colorDepth).substring(0, 32)
    };

    this.authService.verify2Fa(payload).subscribe({
      next: () => {
        this.router.navigate(['/research/timeline']);
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert(err.error?.detail || err.error?.title || 'رمز التحقق غير صحيح.');
      }
    });
  }
}

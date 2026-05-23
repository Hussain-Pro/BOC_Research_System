import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  email: string = '';
  isLoading = false;
  isSuccess = false;
  
  private authService = inject(AuthService);

  onSubmit() {
    if (!this.email) return;
    
    this.isLoading = true;
    
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.isSuccess = true;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert(err.error?.detail || err.error?.title || 'حدث خطأ. قد يكون البريد الإلكتروني غير مسجل.');
      }
    });
  }
}

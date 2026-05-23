import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { BocAuthShellComponent } from '../../shared/boc-auth-shell/boc-auth-shell.component';
import { BocFormFieldComponent } from '../../shared/boc-form-field/boc-form-field.component';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BocAuthShellComponent, BocFormFieldComponent],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent {
  resetPasswordForm: FormGroup;
  isLoading = false;
  isSuccess = false;
  message = '';
  errorMessage = '';
  
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  constructor() {
    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.resetPasswordForm.invalid) return;
    
    this.isLoading = true;
    
    // محاكاة طلب التحديث للاختبار فقط
    setTimeout(() => {
      this.isSuccess = true;
      this.isLoading = false;
      this.message = 'تم إعادة تعيين كلمة المرور بنجاح';
      this.toastService.success(this.message);
    }, 1500);
    
    /*
    const val = this.resetPasswordForm.value;
    this.authService.resetPassword('token_here', val.email_here, val.newPassword).subscribe({
      next: () => {
        this.isSuccess = true;
        this.isLoading = false;
        this.message = 'تم إعادة تعيين كلمة المرور بنجاح';
        this.toastService.success(this.message);
      },
      error: (err) => {
        console.error('Reset Password Error', err);
        this.errorMessage = err.error?.message || 'حدث خطأ أثناء محاولة إعادة تعيين كلمة المرور.';
        this.isLoading = false;
        this.toastService.error(this.errorMessage);
      }
    });
    */
  }
}

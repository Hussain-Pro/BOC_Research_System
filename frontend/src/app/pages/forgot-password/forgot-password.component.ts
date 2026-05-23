import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { BocAuthShellComponent } from '../../shared/boc-auth-shell/boc-auth-shell.component';
import { BocFormFieldComponent } from '../../shared/boc-form-field/boc-form-field.component';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BocAuthShellComponent, BocFormFieldComponent],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  forgotPasswordForm: FormGroup;
  isLoading = false;
  isSuccess = false;
  message = '';
  errorMessage = '';
  
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  constructor() {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit() {
    if (this.forgotPasswordForm.invalid) return;
    
    this.isLoading = true;
    
    // محاكاة إرسال طلب للاختبار فقط
    setTimeout(() => {
      this.isSuccess = true;
      this.isLoading = false;
      this.message = 'تم إرسال رابط الاستعادة بنجاح';
      this.toastService.success('تم إرسال تعليمات استعادة كلمة المرور إلى ' + this.forgotPasswordForm.value.email);
    }, 1500);
    
    /*
    this.authService.forgotPassword(this.forgotPasswordForm.value.email).subscribe({
      next: () => {
        this.isSuccess = true;
        this.isLoading = false;
        this.message = 'تم إرسال رابط الاستعادة بنجاح';
        this.toastService.success('تم إرسال تعليمات استعادة كلمة المرور إلى بريدك الإلكتروني بنجاح');
      },
      error: (err) => {
        console.error('Password reset request error', err);
        this.errorMessage = err.error?.message || 'فشل إرسال طلب استعادة كلمة المرور. يرجى التأكد من صحة البريد الإلكتروني.';
        this.isLoading = false;
        this.toastService.error(this.errorMessage);
      }
    });
    */
  }
}

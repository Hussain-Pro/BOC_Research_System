import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  resetForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    token: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  isLoading = false;

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['email']) {
        this.resetForm.patchValue({ email: params['email'] });
      }
      if (params['token']) {
        this.resetForm.patchValue({ token: params['token'] });
      }
    });
  }

  onSubmit() {
    if (this.resetForm.invalid) return;
    
    this.isLoading = true;
    
    this.authService.resetPassword(this.resetForm.value).subscribe({
      next: () => {
        alert('تم تغيير كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول.');
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        alert(err.error?.detail || err.error?.title || 'فشل في إعادة تعيين كلمة المرور. قد يكون الرابط منتهي الصلاحية.');
      }
    });
  }
}

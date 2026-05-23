import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  errorMessage = '';

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  registerForm: FormGroup = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    nationalID: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    birthDate: ['', Validators.required],
    roleId: ['A7BE60BA-B82D-471F-B521-D89D98BC304C', Validators.required]
  });

  isLoading = false;

  onSubmit() {
    if (this.registerForm.invalid) return;
    
    this.isLoading = true;
    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.toastService.success('تم التسجيل بنجاح. حسابك بانتظار موافقة الموارد البشرية.');
        this.router.navigate(['/auth/login']);
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        this.toastService.error(err.error?.detail || err.error?.title || 'فشل التسجيل. يرجى التحقق من البيانات.');
      }
    });
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { BocAuthShellComponent } from '../../shared/boc-auth-shell/boc-auth-shell.component';
import QRCode from 'qrcode';

@Component({
  selector: 'app-two-factor',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, BocAuthShellComponent],
  templateUrl: './two-factor.component.html',
  styleUrl: './two-factor.component.scss'
})
export class TwoFactorComponent implements OnInit {
  twoFactorForm!: FormGroup;
  email = '';
  requiresSetup = false;
  qrCodeUrl = '';
  secret = '';
  qrDataUrl = '';
  isLoading = false;

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  constructor() {
    this.twoFactorForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
    });
  }

  async ngOnInit() {
    const state = history.state;
    if (state?.email) {
      this.email = state.email;
      this.requiresSetup = state.requiresSetup || false;
      this.qrCodeUrl = state.qrCodeUrl || '';
      this.secret = state.secret || '';
      if (this.requiresSetup && this.qrCodeUrl) {
        await this.generateQrCode();
      }
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  async generateQrCode() {
    try {
      this.qrDataUrl = await QRCode.toDataURL(this.qrCodeUrl, {
        width: 220,
        margin: 2,
        color: { dark: '#0F2A38', light: '#FFFFFF' },
        errorCorrectionLevel: 'M'
      });
    } catch {
      this.toastService.warning('تعذّر توليد رمز QR. يرجى إدخال الرمز يدوياً في التطبيق.');
    }
  }

  copySecret() {
    if (!this.secret) return;
    navigator.clipboard.writeText(this.secret).then(() => {
      this.toastService.success('تم نسخ الرمز السري إلى الحافظة.');
    }).catch(() => {
      this.toastService.warning('تعذّر النسخ التلقائي. يرجى نسخ الرمز يدوياً.');
    });
  }

  onSubmit() {
    if (this.twoFactorForm.invalid) return;
    this.isLoading = true;

    this.authService.verify2Fa({
      email: this.email,
      code: this.twoFactorForm.value.code,
      deviceFingerprint: btoa(navigator.userAgent + navigator.language + screen.colorDepth).substring(0, 32)
    }).subscribe({
      next: () => {
        this.toastService.success('تمت عملية المصادقة بنجاح. مرحباً بك في النظام!');
        const role = this.authService.getRole();
        this.router.navigate([role === 'Admin' ? '/admin/analytics' : '/home']);
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.error(err.error?.detail || err.error?.title || 'رمز التحقق غير صحيح. يرجى المحاولة مجدداً.');
      }
    });
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import { BocAuthShellComponent } from '../../shared/boc-auth-shell/boc-auth-shell.component';
import * as QRCode from 'qrcode';

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
      
      // Fallback: If qrCodeUrl is missing but secret is available, construct the otpauth URL
      if (this.requiresSetup && !this.qrCodeUrl && this.secret) {
        const issuer = encodeURIComponent('BOC_Research');
        const account = encodeURIComponent(this.email);
        this.qrCodeUrl = `otpauth://totp/${issuer}:${account}?secret=${this.secret}&issuer=${issuer}&algorithm=SHA1&digits=6&period=30`;
      }

      if (this.requiresSetup && this.qrCodeUrl) {
        await this.generateQrCode();
      }
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  async generateQrCode() {
    try {
      console.log('Generating QR code with URL:', this.qrCodeUrl);
      const qrLib: any = QRCode;
      console.log('QRCode library import object:', qrLib);
      
      // Resolve toDataURL function from CommonJS / ESM wrapper
      const toDataURLFn = qrLib?.toDataURL || qrLib?.default?.toDataURL || (typeof qrLib === 'function' ? qrLib : null);
      
      if (!toDataURLFn) {
        throw new Error('QRCode.toDataURL function is not resolved from import.');
      }

      this.qrDataUrl = await toDataURLFn(this.qrCodeUrl, {
        width: 220,
        margin: 2,
        color: { dark: '#0F2A38', light: '#FFFFFF' },
        errorCorrectionLevel: 'M'
      });
      console.log('QR Code generated successfully. Length:', this.qrDataUrl?.length);
    } catch (err) {
      console.error('QR Code generation failed:', err);
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

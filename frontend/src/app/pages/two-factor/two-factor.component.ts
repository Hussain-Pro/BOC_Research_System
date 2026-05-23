import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';
import QRCode from 'qrcode';

@Component({
  selector: 'app-two-factor',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './two-factor.component.html',
  styleUrl: './two-factor.component.scss'
})
export class TwoFactorComponent implements OnInit {
  twoFactorForm!: FormGroup;
  email: string = '';
  
  requiresSetup: boolean = false;
  qrCodeUrl: string = '';
  secret: string = '';
  qrDataUrl: string = '';

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
    if (state && state.email) {
      this.email = state.email;
      this.requiresSetup = state.requiresSetup || false;
      this.qrCodeUrl = state.qrCodeUrl || '';
      this.secret = state.secret || '';

      // Generate QR code locally (no external API needed)
      if (this.requiresSetup && this.qrCodeUrl) {
        await this.generateQrCode();
      }
    } else {
      this.router.navigate(['/auth/login']);
    }
  }

  // Generate QR code as a base64 data URL using the qrcode library
  async generateQrCode() {
    try {
      this.qrDataUrl = await QRCode.toDataURL(this.qrCodeUrl, {
        width: 220,
        margin: 2,
        color: {
          dark: '#0F2A38',
          light: '#FFFFFF'
        },
        errorCorrectionLevel: 'M'
      });
    } catch (err) {
      console.error('QR Code generation failed:', err);
      this.toastService.warning('تعذّر توليد رمز QR. يرجى إدخال الرمز يدوياً في التطبيق.');
    }
  }

  copySecret() {
    if (this.secret) {
      navigator.clipboard.writeText(this.secret).then(() => {
        this.toastService.success('تم نسخ الرمز السري إلى الحافظة.');
      }).catch(() => {
        this.toastService.warning('تعذّر النسخ التلقائي. يرجى نسخ الرمز يدوياً.');
      });
    }
  }

  onSubmit() {
    if (this.twoFactorForm.invalid) return;

    this.isLoading = true;

    const payload = {
      email: this.email,
      code: this.twoFactorForm.value.code,
      deviceFingerprint: btoa(navigator.userAgent + navigator.language + screen.colorDepth).substring(0, 32)
    };

    this.authService.verify2Fa(payload).subscribe({
      next: () => {
        this.toastService.success('تمت عملية المصادقة بنجاح. مرحباً بك في النظام!');
        const role = this.authService.getRole();
        if (role === 'Admin') {
          this.router.navigate(['/admin/analytics']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
        this.toastService.error(err.error?.detail || err.error?.title || 'رمز التحقق غير صحيح. يرجى المحاولة مجدداً.');
      }
    });
  }

  resendCode(event: Event) {
    event.preventDefault();
    this.toastService.info('تم إرسال رمز جديد إلى بريدك الإلكتروني.');
  }
}

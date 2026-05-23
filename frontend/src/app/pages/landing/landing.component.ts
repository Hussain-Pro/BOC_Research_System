import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingPageComponent {
  currentYear = new Date().getFullYear();

  onboardingSteps = [
    { icon: 'bi-person-plus', title: 'إنشاء الحساب', desc: 'قم بتسجيل حساب جديد لإنشاء ملفك الشخصي كباحث في النظام.', bg: 'rgba(15,42,56,0.1)', color: 'var(--boc-primary)' },
    { icon: 'bi-file-earmark-arrow-up', title: 'تقديم البحث', desc: 'ارفع ملف بحثك مع الملخص والمعلومات الأساسية ليتم تحويله للجنة.', bg: 'rgba(40,167,69,0.1)', color: 'var(--boc-success)' },
    { icon: 'bi-search', title: 'المراجعة والتقييم', desc: 'يقوم المقيمون بمراجعة بحثك وإبداء الملاحظات والتعديلات المطلوبة.', bg: 'rgba(255,193,7,0.15)', color: '#856404' },
    { icon: 'bi-check-circle', title: 'الاعتماد النهائي', desc: 'بعد استيفاء الشروط وإكمال التعديلات، يتم اعتماد البحث رسمياً.', bg: 'rgba(23,162,184,0.12)', color: 'var(--boc-info)' }
  ];
}

import { Component, OnDestroy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ResearchService } from '../../services/research.service';
import { BocLayoutService } from '../../services/boc-layout.service';
import { BocFormFieldComponent } from '../../shared/boc-form-field/boc-form-field.component';
import { BocVerticalTimelineComponent, BocTimelineStep } from '../../shared/boc-vertical-timeline/boc-vertical-timeline.component';

@Component({
  selector: 'app-submit-research',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatCheckboxModule,
    BocVerticalTimelineComponent
  ],
  templateUrl: './submit-research.component.html',
  styleUrl: './submit-research.component.scss'
})
export class SubmitResearchComponent implements OnInit, OnDestroy {
  @ViewChild('stepper') stepper!: MatStepper;

  private fb = inject(FormBuilder);
  private layoutService = inject(BocLayoutService);
  researchService = inject(ResearchService);
  router = inject(Router);

  departmentId = '3F2B1A9E-8D7C-6B5A-4928-1029384756AD';
  directorateId = '1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D';
  researcherId = '9F8E7D6C-5B4A-3F2E-1D0C-9B8A7F6E5D4C';

  detailsForm = this.fb.group({
    title: ['', Validators.required],
    abstract: [''],
    submitImmediately: [true]
  });

  selectedFile: File | null = null;
  error = signal<string | null>(null);
  success = signal<boolean>(false);
  trackingNumber = signal<string>('');

  successTimelineSteps: BocTimelineStep[] = [
    { title: 'مسودة', description: 'تم حفظ البحث كمسودة.', isCompleted: true, isActive: false, date: new Date() },
    { title: 'تدقيق السكرتير', description: 'الباحث في انتظار التدقيق المبدئي من السكرتير.', isCompleted: false, isActive: true },
    { title: 'قيد الفرز', description: 'سيتم إرسال البحث لفرز اللجنة بعد اجتياز التدقيق.', isCompleted: false, isActive: false },
    { title: 'التقييم والاعتماد', description: 'مرحلة التقييم العلمي وإصدار المحضر النهائي.', isCompleted: false, isActive: false }
  ];

  ngOnInit(): void {
    this.layoutService.setPage('تقديم بحث جديد', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'تقديم بحث' }
    ]);
  }

  ngOnDestroy(): void {
    this.layoutService.clearPage();
  }

  onFileSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  onSubmit(): void {
    if (!this.selectedFile) {
      this.error.set('يرجى اختيار ملف البحث.');
      return;
    }

    this.error.set(null);
    const { title, abstract, submitImmediately } = this.detailsForm.getRawValue();
    const formData = new FormData();
    formData.append('title', title ?? '');
    formData.append('abstract', abstract ?? '');
    formData.append('categoryId', '');
    formData.append('researcherId', this.researcherId);
    formData.append('departmentId', this.departmentId);
    formData.append('directorateId', this.directorateId);
    formData.append('submitImmediately', String(submitImmediately));
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.researchService.submitResearch(formData).subscribe({
      next: (res) => {
        this.success.set(true);
        const tracking = typeof res === 'string'
          ? res
          : (res as { trackingNumber?: string })?.trackingNumber;
        this.trackingNumber.set(tracking || 'BOC-RES-2026-SUCCESS');
        this.stepper.next();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'حدث خطأ أثناء رفع البحث. يرجى التأكد من اتصال الشبكة وصلاحية الملف.');
      }
    });
  }
}

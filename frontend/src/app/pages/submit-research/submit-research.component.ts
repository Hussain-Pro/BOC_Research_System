import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ResearchService } from '../../services/research.service';

@Component({
  selector: 'app-submit-research',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './submit-research.component.html',
  styleUrl: './submit-research.component.scss'
})
export class SubmitResearchComponent {
  title = '';
  abstract = '';
  categoryId = '';
  departmentId = '3F2B1A9E-8D7C-6B5A-4928-1029384756AD'; // Mock Department ID
  directorateId = '1A2B3C4D-5E6F-7A8B-9C0D-1E2F3A4B5C6D'; // Mock Directorate ID
  researcherId = '9F8E7D6C-5B4A-3F2E-1D0C-9B8A7F6E5D4C'; // Mock Researcher ID
  submitImmediately = true;
  selectedFile: File | null = null;

  step = signal<number>(1);
  error = signal<string | null>(null);
  success = signal<boolean>(false);
  trackingNumber = signal<string>('');

  // 14 Operational States for Timeline representation
  statesList = [
    { label: 'مسودة', name: 'Draft' },
    { label: 'تدقيق السكرتير', name: 'Pending_Secretary_Screening' },
    { label: 'غير مطابق', name: 'Non_Compliant_Returned' },
    { label: 'قيد الفرز', name: 'Incoming_Triage_Queue' },
    { label: 'مرسل للمقيمين', name: 'Dispatched_To_Evaluators' },
    { label: 'درجة رئيس اللجنة', name: 'Pending_Chairman_Grading' },
    { label: 'مستبدل', name: 'Substituted' },
    { label: 'موقوف انتحال', name: 'Suspended_Plagiarism_Lockout' },
    { label: 'احالة تقاعد', name: 'Force_Majeure_Retired' },
    { label: 'وفاة الباحث', name: 'Force_Majeure_Deceased' },
    { label: 'وجبة الوزارة', name: 'Ministry_Batch_Transit' },
    { label: 'ناجح مستوف', name: 'Pass_Approved' },
    { label: 'غير ناجح', name: 'Fail_Rejected' },
    { label: 'مؤرشف', name: 'Archived' }
  ];

  currentState = 'Pending_Secretary_Screening';

  constructor(public researchService: ResearchService, public router: Router) {}

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  nextStep() {
    if (this.step() < 2) {
      this.step.update(s => s + 1);
    }
  }

  prevStep() {
    if (this.step() > 1) {
      this.step.update(s => s - 1);
    }
  }

  onSubmit() {
    if (!this.selectedFile) {
      this.error.set('يرجى اختيار ملف البحث.');
      return;
    }

    this.error.set(null);
    const formData = new FormData();
    formData.append('title', this.title);
    formData.append('abstract', this.abstract);
    formData.append('categoryId', this.categoryId);
    formData.append('researcherId', this.researcherId);
    formData.append('departmentId', this.departmentId);
    formData.append('directorateId', this.directorateId);
    formData.append('submitImmediately', this.submitImmediately.toString());
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.researchService.submitResearch(formData).subscribe({
      next: (res: any) => {
        this.success.set(true);
        this.trackingNumber.set(res?.trackingNumber || 'BOC-RES-2026-SUCCESS');
        this.step.set(3);
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'حدث خطأ أثناء رفع البحث. يرجى التأكد من اتصال الشبكة وصلاحية الملف.');
      }
    });
  }
}

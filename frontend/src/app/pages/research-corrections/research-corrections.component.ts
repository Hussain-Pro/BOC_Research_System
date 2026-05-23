import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { BocLayoutService } from '../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../shared/boc-page-hero/boc-page-hero.component';
import { BocGlassCardComponent } from '../../shared/boc-glass-card/boc-glass-card.component';

@Component({
  selector: 'app-research-corrections',
  standalone: true,
  imports: [CommonModule, RouterModule, BocPageHeroComponent, BocGlassCardComponent],
  templateUrl: './research-corrections.component.html',
  styleUrl: './research-corrections.component.scss'
})
export class ResearchCorrectionsComponent implements OnInit, OnDestroy {
  private toastService = inject(ToastService);
  private layoutService = inject(BocLayoutService);

  research = {
    id: 'R-10045',
    title: 'تأثير الضخ العميق على الآبار القديمة في حقل الرميلة',
    status: 'Non_Compliant_Returned',
    secretaryNotes: 'يرجى الالتزام بالخط (Arial 14) وإضافة ملخص تنفيذي لا يتجاوز 250 كلمة. المراجع غير مكتوبة بصيغة APA.',
    returnedAt: '2026-05-20T10:00:00Z',
    correctionRound: 1
  };

  selectedFile: File | null = null;
  isSubmitting = false;
  isDragOver = false;

  ngOnInit(): void {
    this.layoutService.setPage('إرسال تعديلات البحث', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'تاريخ الأبحاث', route: '/research/history' },
      { label: 'التعديلات' }
    ]);
  }

  ngOnDestroy(): void {
    this.layoutService.clearPage();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      this.validateAndSetFile(file, input);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.validateAndSetFile(file);
    }
  }

  private validateAndSetFile(file: File, input?: HTMLInputElement): void {
    if (file.size > 50 * 1024 * 1024) {
      this.toastService.error('حجم الملف يتجاوز الحد الأقصى (50 ميجابايت).');
      this.selectedFile = null;
      if (input) input.value = '';
      return;
    }
    this.selectedFile = file;
  }

  submitCorrection(): void {
    if (!this.selectedFile) {
      this.toastService.error('يرجى إرفاق النسخة المعدلة من البحث.');
      return;
    }

    this.isSubmitting = true;

    setTimeout(() => {
      this.isSubmitting = false;
      this.toastService.success('تم رفع النسخة المعدلة بنجاح. سيتم مراجعتها من قبل السكرتير.');
      this.selectedFile = null;
    }, 2000);
  }
}

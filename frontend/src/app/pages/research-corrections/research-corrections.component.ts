import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-research-corrections',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './research-corrections.component.html',
  styleUrls: ['./research-corrections.component.scss']
})
export class ResearchCorrectionsComponent implements OnInit {
  
  // Mock data for the paper needing correction
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

  constructor(private toastService: ToastService) {}

  ngOnInit(): void {
    // In a real app, we would fetch the specific research details from the API using an ID from the route.
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (file.size > 50 * 1024 * 1024) {
        this.toastService.error('حجم الملف يتجاوز الحد الأقصى (50 ميجابايت).');
        this.selectedFile = null;
        event.target.value = '';
        return;
      }
      this.selectedFile = file;
    }
  }

  submitCorrection() {
    if (!this.selectedFile) {
      this.toastService.error('يرجى إرفاق النسخة المعدلة من البحث.');
      return;
    }

    this.isSubmitting = true;

    // Simulate API Call for uploading correction
    setTimeout(() => {
      this.isSubmitting = false;
      this.toastService.success('تم رفع النسخة المعدلة بنجاح. سيتم مراجعتها من قبل السكرتير.');
      this.selectedFile = null;
      // In a real app, redirect to timeline
    }, 2000);
  }
}

import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { PdfViewerModule } from 'ng2-pdf-viewer';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-committee-workspace',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PdfViewerModule],
  templateUrl: './committee-workspace.component.html',
  styleUrls: ['./committee-workspace.component.scss']
})
export class CommitteeWorkspaceComponent implements OnInit {
  authService = inject(AuthService);
  toastService = inject(ToastService);

  userRole: string = '';
  
  // Mock Active Assignments
  assignments = [
    { id: 'A-1001', researchId: 'R-10045', title: 'تأثير الضخ العميق في الآبار', assignedDate: '2026-05-20', dueDate: '2026-06-03', status: 'Active' },
    { id: 'A-1002', researchId: 'R-09950', title: 'تحليل البيانات السيزمية', assignedDate: '2026-05-15', dueDate: '2026-05-29', status: 'Active' }
  ];

  selectedAssignment: any = null;
  
  // Evaluation form state
  evaluationScore: number | null = null;
  evaluationComments: string = '';
  isSubmitting = false;

  // PDF Viewer State
  pdfSrc = 'assets/mock-research.pdf'; // In real app: proxy URL with FileAccessToken
  zoom = 1.0;
  pdfLoading = false;

  ngOnInit() {
    this.userRole = this.authService.getRole();
  }

  selectAssignment(assignment: any) {
    this.selectedAssignment = assignment;
    this.evaluationScore = null;
    this.evaluationComments = '';
    this.pdfLoading = true;
    
    // Reset zoom
    this.zoom = 1.0;
  }

  zoomIn() {
    this.zoom += 0.2;
  }

  zoomOut() {
    this.zoom -= 0.2;
  }

  downloadPdf() {
    const allowedRoles = ['Evaluator', 'Member', 'Chairman', 'Secretary', 'Admin', 'Deputy'];
    if (allowedRoles.includes(this.userRole)) {
      // Create a dummy link to trigger download
      const link = document.createElement('a');
      link.href = this.pdfSrc;
      link.download = `Research_${this.selectedAssignment.researchId}.pdf`;
      link.click();
      this.toastService.success('تم بدء تنزيل الملف.');
    } else {
      this.toastService.error('لا تملك صلاحية تنزيل الملف.');
    }
  }

  submitEvaluation() {
    if (this.evaluationScore === null || this.evaluationScore < 0 || this.evaluationScore > 100) {
      this.toastService.error('الرجاء إدخال درجة صحيحة (0 - 100).');
      return;
    }

    if (!this.evaluationComments || this.evaluationComments.trim().length < 20) {
      this.toastService.error('الرجاء كتابة تقرير فني وافٍ (20 حرف على الأقل).');
      return;
    }

    this.isSubmitting = true;
    setTimeout(() => {
      this.isSubmitting = false;
      this.toastService.success('تم حفظ التقييم بنجاح وإرساله للجنة.');
      
      // Remove from active queue
      this.assignments = this.assignments.filter(a => a.id !== this.selectedAssignment.id);
      this.selectedAssignment = null;
    }, 1500);
  }

  getDaysRemaining(dueDate: string): number {
    const today = new Date();
    const due = new Date(dueDate);
    const diffTime = Math.abs(due.getTime() - today.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }
}

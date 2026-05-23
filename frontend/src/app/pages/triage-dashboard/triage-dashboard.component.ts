import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TriageService, TriagePaper, EligibleEvaluator } from '../../services/triage.service';
import { SignalRService } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';
import { Subscription } from 'rxjs';

import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-triage-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './triage-dashboard.component.html',
  styleUrl: './triage-dashboard.component.scss'
})
export class TriageDashboardComponent implements OnInit, OnDestroy {
  triagePapers = signal<TriagePaper[]>([]);
  selectedPaper = signal<TriagePaper | null>(null);
  eligibleEvaluators = signal<EligibleEvaluator[]>([]);
  safeDocumentUrl: SafeResourceUrl | null = null;
  
  // Selection models
  selectedEvaluatorIds: string[] = [];
  selectedMemberIds: string[] = [];

  // Feedback notifications
  success = signal<boolean>(false);
  error = signal<string | null>(null);
  infoMessage = signal<string | null>(null);
  isLoading = signal<boolean>(false);
  isLoadingEvaluators = signal<boolean>(false);

  private subscriptions = new Subscription();

  constructor(
    private triageService: TriageService,
    private signalRService: SignalRService,
    private authService: AuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.loadPapers();
    this.setupRealTimeListeners();
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  loadPapers() {
    this.isLoading.set(true);
    this.triageService.getTriagePapers().subscribe({
      next: (papers) => {
        this.triagePapers.set(papers);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('فشل تحميل قائمة البحوث قيد الفرز.');
        this.isLoading.set(false);
      }
    });
  }

  setupRealTimeListeners() {
    // Listen for SignalR state change events
    this.subscriptions.add(
      this.signalRService.paperStatusUpdates$.subscribe((update) => {
        if (update.newState === 'Incoming_Triage_Queue') {
          this.infoMessage.set('تنبيه: وصل بحث جديد إلى قائمة الفرز!');
          this.loadPapers();
          setTimeout(() => this.infoMessage.set(null), 5000);
        }
      })
    );

    // Listen for other real-time notifications
    this.subscriptions.add(
      this.signalRService.notifications$.subscribe((notif) => {
        this.infoMessage.set(`إشعار: ${notif.title} - ${notif.message}`);
        setTimeout(() => this.infoMessage.set(null), 6000);
      })
    );
  }

  selectPaper(paper: TriagePaper) {
    this.selectedPaper.set(paper);
    this.selectedEvaluatorIds = [];
    this.selectedMemberIds = [];
    this.error.set(null);
    this.success.set(false);
    this.safeDocumentUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getDocumentUrl(paper.id));
    this.loadEvaluators(paper.id);
  }

  loadEvaluators(paperId: string) {
    this.isLoadingEvaluators.set(true);
    this.triageService.getEligibleEvaluators(paperId).subscribe({
      next: (evaluators) => {
        this.eligibleEvaluators.set(evaluators);
        this.isLoadingEvaluators.set(false);
      },
      error: (err) => {
        this.error.set('خطأ أثناء تحميل المقيمين المؤهلين.');
        this.isLoadingEvaluators.set(false);
      }
    });
  }

  toggleEvaluator(evaluatorId: string) {
    const index = this.selectedEvaluatorIds.indexOf(evaluatorId);
    if (index > -1) {
      this.selectedEvaluatorIds.splice(index, 1);
    } else {
      this.selectedEvaluatorIds.push(evaluatorId);
    }
  }

  toggleMember(memberId: string) {
    const index = this.selectedMemberIds.indexOf(memberId);
    if (index > -1) {
      this.selectedMemberIds.splice(index, 1);
    } else {
      this.selectedMemberIds.push(memberId);
    }
  }

  onSubmitAssignment() {
    const paper = this.selectedPaper();
    if (!paper) return;

    if (this.selectedEvaluatorIds.length === 0 && this.selectedMemberIds.length === 0) {
      this.error.set('يرجى اختيار مقيم واحد على الأقل أو عضو لجنة.');
      return;
    }

    this.error.set(null);
    this.success.set(false);

    const currentUser = this.authService.currentUser();
    const mappedById = currentUser?.nameid || 'D2A9B10E-8D7C-6B5A-4928-1029384756AE'; // Fallback for dev

    const payload = {
      researchId: paper.id,
      mappedById: mappedById,
      evaluatorIds: this.selectedEvaluatorIds,
      memberIds: this.selectedMemberIds
    };

    this.triageService.assignEvaluators(payload).subscribe({
      next: (res) => {
        this.success.set(true);
        this.selectedPaper.set(null);
        this.loadPapers();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'فشل إتمام عملية التوزيع والإسناد.');
      }
    });
  }

  // Secure FTP Stream URL provider
  getDocumentUrl(paperId: string): string {
    return `https://localhost:7143/api/research/${paperId}/document`;
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../../services/toast.service';
import { BocLayoutService } from '../../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../../shared/boc-page-hero/boc-page-hero.component';
import { BocStatCardComponent } from '../../../shared/boc-stat-card/boc-stat-card.component';
import { BocGlassCardComponent } from '../../../shared/boc-glass-card/boc-glass-card.component';
import { BocEmptyStateComponent } from '../../../shared/boc-empty-state/boc-empty-state.component';

@Component({
  selector: 'app-plagiarism-override',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocPageHeroComponent,
    BocStatCardComponent,
    BocGlassCardComponent,
    BocEmptyStateComponent
  ],
  templateUrl: './plagiarism-override.component.html',
  styleUrls: ['./plagiarism-override.component.scss']
})
export class PlagiarismOverrideComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  lockedResearches = [
    {
      id: 'R-09999',
      title: 'تحليل مكونات التربة السطحية',
      researcher: 'يوسف العلي',
      plagiarismScore: 45,
      lockedDate: '2026-05-21',
      systemNotes: 'تجاوز نسبة الاستلال المسموحة. تم إيقاف البحث تلقائياً.'
    }
  ];

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
    this.layoutService.setPage('رفع حظر الانتحال العلمي', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'تجاوز الانتحال' }
    ]);
  }

  overrideLockout(researchId: string) {
    if (confirm('هل أنت متأكد من رفع الحظر عن هذا البحث؟ هذا الإجراء يجب أن يتم بطلب رسمي.')) {
      this.toastService.success(`تم رفع الحظر عن البحث ${researchId} وهو الآن متاح للفرز.`);
      this.lockedResearches = this.lockedResearches.filter(r => r.id !== researchId);
    }
  }

  rejectPermanently(researchId: string) {
    if (confirm('هل أنت متأكد من رفض البحث نهائياً بسبب الانتحال العلمي؟')) {
      this.toastService.error(`تم الرفض النهائي للبحث ${researchId} وإشعار الباحث.`);
      this.lockedResearches = this.lockedResearches.filter(r => r.id !== researchId);
    }
  }
}

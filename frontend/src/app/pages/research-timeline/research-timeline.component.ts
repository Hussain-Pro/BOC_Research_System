import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TimelineStep {
  title: string;
  description: string;
  date?: Date;
  isCompleted: boolean;
  isActive: boolean;
  requiresAction?: boolean;
}

@Component({
  selector: 'app-research-timeline',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './research-timeline.component.html',
  styleUrl: './research-timeline.component.scss'
})
export class ResearchTimelineComponent implements OnInit {
  researchTitle = 'تطوير حقن المياه في حقول الرميلة';
  referenceNumber = 'BOC-RES-2026-0084';
  currentStatus = 'قيد التقييم (رئيس اللجنة)';
  
  timelineSteps: TimelineStep[] = [];

  ngOnInit() {
    // Mock Data for the blueprint timeline
    this.timelineSteps = [
      {
        title: 'تقديم البحث',
        description: 'تم رفع البحث ومستمسكات الفحص بنجاح من قبل الباحث.',
        date: new Date('2026-05-18T09:30:00'),
        isCompleted: true,
        isActive: false
      },
      {
        title: 'فرز السكرتارية (Screening)',
        description: 'تم التدقيق المبدئي للبحث وخلوه من الاستلال.',
        date: new Date('2026-05-19T11:15:00'),
        isCompleted: true,
        isActive: false
      },
      {
        title: 'توزيع المقيمين (Triage)',
        description: 'تم تكليف 3 مقيمين لتقييم البحث (قيد الانتظار لموافقة المقيمين).',
        date: new Date('2026-05-20T14:00:00'),
        isCompleted: true,
        isActive: false
      },
      {
        title: 'التقييم العلمي والتسجيل (Grading)',
        description: 'يقوم المقيمون ورئيس اللجنة حالياً بوضع الدرجات للبحث (70/30).',
        isCompleted: false,
        isActive: true,
        requiresAction: false
      },
      {
        title: 'الاعتماد وإصدار المحضر (Resolution)',
        description: 'تحويل نتيجة البحث إلى محضر رسمي موثق.',
        isCompleted: false,
        isActive: false
      }
    ];
  }
}

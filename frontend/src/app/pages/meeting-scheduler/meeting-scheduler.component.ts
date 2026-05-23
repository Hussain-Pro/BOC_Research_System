import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { BocLayoutService } from '../../services/boc-layout.service';
import {
  BocPageHeroComponent,
  BocGlassCardComponent,
  BocStatCardComponent,
  BocEmptyStateComponent
} from '../../shared';

@Component({
  selector: 'app-meeting-scheduler',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    BocPageHeroComponent,
    BocGlassCardComponent,
    BocStatCardComponent,
    BocEmptyStateComponent
  ],
  templateUrl: './meeting-scheduler.component.html',
  styleUrls: ['./meeting-scheduler.component.scss']
})
export class MeetingSchedulerComponent implements OnInit {
  private toastService = inject(ToastService);
  private layoutService = inject(BocLayoutService);

  meetingDate = '';
  meetingTime = '';
  meetingLocation = '';

  agendaItems: unknown[] = [];

  availableResearches = [
    { id: 'R-10045', title: 'تأثير الضخ العميق', selected: false },
    { id: 'R-09950', title: 'تحليل البيانات السيزمية', selected: false },
    { id: 'R-09820', title: 'تقييم المخاطر البيئية', selected: false }
  ];

  isSubmitting = false;

  breadcrumbs = [
    { label: 'الرئيسية', route: '/home' },
    { label: 'جدولة الاجتماعات' }
  ];

  ngOnInit(): void {
    this.layoutService.setPage('جدولة الاجتماعات');
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.meetingDate = tomorrow.toISOString().split('T')[0];
    this.meetingTime = '10:00';
    this.meetingLocation = 'قاعة الاجتماعات الرئيسية - الطابق الثاني';
  }

  get selectedAgendaCount(): number {
    return this.availableResearches.filter(r => r.selected).length;
  }

  scheduleMeeting() {
    if (!this.meetingDate || !this.meetingTime || !this.meetingLocation) {
      this.toastService.error('يرجى تعبئة جميع الحقول الأساسية.');
      return;
    }

    const selectedAgendas = this.availableResearches.filter(r => r.selected);
    if (selectedAgendas.length === 0) {
      this.toastService.error('يرجى تحديد بحث واحد على الأقل ليتم إدراجه في جدول الأعمال.');
      return;
    }

    this.isSubmitting = true;

    setTimeout(() => {
      this.isSubmitting = false;
      this.toastService.success('تم جدولة الاجتماع بنجاح وإرسال الدعوات (RSVP) لأعضاء اللجنة.');
      this.availableResearches.forEach(r => r.selected = false);
    }, 1500);
  }
}

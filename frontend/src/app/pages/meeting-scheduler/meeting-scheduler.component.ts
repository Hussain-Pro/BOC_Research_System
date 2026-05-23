import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-meeting-scheduler',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './meeting-scheduler.component.html',
  styleUrls: ['./meeting-scheduler.component.scss']
})
export class MeetingSchedulerComponent implements OnInit {

  // Form Model
  meetingDate: string = '';
  meetingTime: string = '';
  meetingLocation: string = '';
  
  // Agendas
  agendaItems: any[] = [];
  
  // Available Evaluated Researches to include in agenda
  availableResearches = [
    { id: 'R-10045', title: 'تأثير الضخ العميق', selected: false },
    { id: 'R-09950', title: 'تحليل البيانات السيزمية', selected: false },
    { id: 'R-09820', title: 'تقييم المخاطر البيئية', selected: false }
  ];

  isSubmitting = false;

  constructor(private toastService: ToastService) { }

  ngOnInit(): void {
    // Set default date to tomorrow
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.meetingDate = tomorrow.toISOString().split('T')[0];
    this.meetingTime = '10:00';
    this.meetingLocation = 'قاعة الاجتماعات الرئيسية - الطابق الثاني';
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

    // Simulate API request
    setTimeout(() => {
      this.isSubmitting = false;
      this.toastService.success('تم جدولة الاجتماع بنجاح وإرسال الدعوات (RSVP) لأعضاء اللجنة.');
      // Reset form
      this.availableResearches.forEach(r => r.selected = false);
    }, 1500);
  }
}

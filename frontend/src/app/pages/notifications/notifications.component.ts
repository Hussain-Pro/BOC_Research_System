import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface Notification {
  id: string;
  title: string;
  message: string;
  timestamp: Date;
  isRead: boolean;
  type: 'INFO' | 'WARNING' | 'SUCCESS' | 'URGENT';
  actionUrl?: string;
}

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  notifications: Notification[] = [];

  ngOnInit(): void {
    // Mock incoming SignalR Notifications
    this.notifications = [
      {
        id: '1',
        title: 'تحذير تجاوز مدة التقييم (SLA)',
        message: 'البحث "تطوير حقن المياه" تجاوز مدة التقييم بـ 3 أيام.',
        timestamp: new Date(),
        isRead: false,
        type: 'URGENT',
        actionUrl: '/research/timeline'
      },
      {
        id: '2',
        title: 'تم رفع قرار اللجنة النهائي',
        message: 'تمت المصادقة على محضر الاجتماع ذي الرقم BOC-2026-99.',
        timestamp: new Date(Date.now() - 3600000),
        isRead: true,
        type: 'SUCCESS',
        actionUrl: '/meetings/studio'
      },
      {
        id: '3',
        title: 'دعوة تقييم جديدة',
        message: 'تم تكليفك بتقييم بحث جديد في قسم الحفر.',
        timestamp: new Date(Date.now() - 86400000),
        isRead: true,
        type: 'INFO',
        actionUrl: '/meetings/rsvp'
      }
    ];
  }

  markAllAsRead() {
    this.notifications.forEach(n => n.isRead = true);
  }

  getIconClass(type: string): string {
    switch (type) {
      case 'URGENT': return 'bg-danger text-white';
      case 'SUCCESS': return 'bg-success text-white';
      case 'WARNING': return 'bg-warning text-dark';
      default: return 'bg-info text-white';
    }
  }

  getIcon(type: string): string {
    switch (type) {
      case 'URGENT': return 'bi-exclamation-triangle-fill';
      case 'SUCCESS': return 'bi-check-circle-fill';
      case 'WARNING': return 'bi-exclamation-circle-fill';
      default: return 'bi-info-circle-fill';
    }
  }
}

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocPageHeroComponent } from '../../shared/boc-page-hero/boc-page-hero.component';
import { BocGlassCardComponent } from '../../shared/boc-glass-card/boc-glass-card.component';
import { BocEmptyStateComponent } from '../../shared/boc-empty-state/boc-empty-state.component';
import { BocLayoutService } from '../../services/boc-layout.service';

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
  imports: [CommonModule, RouterModule, BocPageHeroComponent, BocGlassCardComponent, BocEmptyStateComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent implements OnInit {
  private layoutService = inject(BocLayoutService);
  notifications: Notification[] = [];

  ngOnInit(): void {
    this.layoutService.setPage('مركز الإشعارات');
    this.notifications = [
      { id: '1', title: 'تحذير تجاوز مدة التقييم (SLA)', message: 'البحث "تطوير حقن المياه" تجاوز مدة التقييم بـ 3 أيام.', timestamp: new Date(), isRead: false, type: 'URGENT', actionUrl: '/research/history' },
      { id: '2', title: 'تم رفع قرار اللجنة النهائي', message: 'تمت المصادقة على محضر الاجتماع BOC-2026-99.', timestamp: new Date(Date.now() - 3600000), isRead: true, type: 'SUCCESS', actionUrl: '/committee/scheduler' },
      { id: '3', title: 'دعوة تقييم جديدة', message: 'تم تكليفك بتقييم بحث جديد في قسم الحفر.', timestamp: new Date(Date.now() - 86400000), isRead: true, type: 'INFO', actionUrl: '/committee/rsvp' }
    ];
  }

  markAllAsRead() { this.notifications.forEach(n => n.isRead = true); }

  getIconClass(type: string): string {
    switch (type) {
      case 'URGENT': return 'urgent';
      case 'SUCCESS': return 'success';
      case 'WARNING': return 'warning';
      default: return 'info';
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

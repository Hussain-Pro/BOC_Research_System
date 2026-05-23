import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocLayoutService } from '../../../services/boc-layout.service';
import { BocPageHeroComponent } from '../../../shared/boc-page-hero/boc-page-hero.component';
import { BocGlassCardComponent } from '../../../shared/boc-glass-card/boc-glass-card.component';

@Component({
  selector: 'app-system-config',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    BocPageHeroComponent,
    BocGlassCardComponent
  ],
  templateUrl: './system-config.component.html',
  styleUrls: ['./system-config.component.scss']
})
export class SystemConfigComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  slaDays = 10;
  warningDays = 3;
  tempRetention = 24;

  departments = [
    { id: 1, name: 'قسم هندسة المكامن وتطوير الحقول' },
    { id: 2, name: 'قسم الحفر والاستصلاح' }
  ];

  constructor() { }

  ngOnInit(): void {
    this.layoutService.setPage('إعدادات النظام', [
      { label: 'الرئيسية', route: '/home' },
      { label: 'إعدادات النظام' }
    ]);
  }
}

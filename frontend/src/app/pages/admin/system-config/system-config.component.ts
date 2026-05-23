import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-system-config',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './system-config.component.html',
  styleUrls: ['./system-config.component.scss']
})
export class SystemConfigComponent implements OnInit {

  slaDays = 10;
  warningDays = 3;
  tempRetention = 24;

  departments = [
    { id: 1, name: 'قسم هندسة المكامن وتطوير الحقول' },
    { id: 2, name: 'قسم الحفر والاستصلاح' }
  ];

  constructor() { }

  ngOnInit(): void {
  }
}

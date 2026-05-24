import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BocLayoutService } from '../../services/boc-layout.service';


@Component({
  selector: 'app-rsvp',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rsvp.component.html',
  styleUrl: './rsvp.component.scss'
})
export class RsvpComponent implements OnInit {
  private layoutService = inject(BocLayoutService);

  evaluatorName = 'د. أحمد المحمداوي';
  researchTitle = 'دراسة تأثير التآكل على أنابيب النقل في حقل مجنون';
  referenceNumber = 'BOC-RES-2026-0092';

  hasResponded = false;
  isAccepted = false;

  breadcrumbs = [
    { label: 'الرئيسية', route: '/home' },
    { label: 'دعوة التقييم' }
  ];

  ngOnInit(): void {
    this.layoutService.setPage('دعوة التقييم');
  }

  respond(accept: boolean) {
    this.isAccepted = accept;
    this.hasResponded = true;
  }
}

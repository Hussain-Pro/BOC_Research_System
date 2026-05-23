import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-rsvp',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './rsvp.component.html',
  styleUrl: './rsvp.component.scss'
})
export class RsvpComponent implements OnInit {
  evaluatorName = 'د. أحمد المحمداوي';
  researchTitle = 'دراسة تأثير التآكل على أنابيب النقل في حقل مجنون';
  referenceNumber = 'BOC-RES-2026-0092';
  
  hasResponded = false;
  isAccepted = false;

  ngOnInit(): void {
    // Check router params or API to see if already responded
  }

  respond(accept: boolean) {
    // API Call to Triage Controller to update Assignment State
    this.isAccepted = accept;
    this.hasResponded = true;
  }
}

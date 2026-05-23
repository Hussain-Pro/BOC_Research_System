import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './access-denied.component.html',
  styles: [`
    .btn-primary-industrial {
      background-color: var(--boc-primary);
      border-color: var(--boc-primary);
      color: white;
    }
    .btn-primary-industrial:hover {
      background-color: var(--boc-primary-light);
      border-color: var(--boc-primary-light);
      color: white;
    }
  `]
})
export class AccessDeniedComponent {

}

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './not-found.component.html',
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
export class NotFoundComponent {

}

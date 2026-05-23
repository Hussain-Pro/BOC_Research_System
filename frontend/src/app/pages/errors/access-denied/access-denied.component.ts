import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BocAuthShellComponent } from '../../../shared/boc-auth-shell/boc-auth-shell.component';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [CommonModule, RouterModule, BocAuthShellComponent],
  templateUrl: './access-denied.component.html'
})
export class AccessDeniedComponent {}

import { Component, inject, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  authService = inject(AuthService);
  router = inject(Router);

  @Output() sidebarToggled = new EventEmitter<boolean>();
  isSidebarOpen = false;

  get isLoggedIn(): boolean {
    return this.authService.isAuthenticated();
  }

  get userRole(): string {
    return this.authService.getRole();
  }

  get userName(): string {
    const user = this.authService.currentUser();
    return user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'مستخدم النظام';
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
    this.sidebarToggled.emit(this.isSidebarOpen);
  }

  logout() {
    this.authService.logout();
  }
}

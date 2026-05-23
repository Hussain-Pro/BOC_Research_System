import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ToastContainerComponent } from './components/toast-container/toast-container.component';
import { NavbarComponent } from './components/navbar/navbar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, ToastContainerComponent, NavbarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'boc-research';
  router = inject(Router);

  ngOnInit(): void {
    const storedTheme = localStorage.getItem('boc_theme');
    if (storedTheme === 'dark') {
      document.body.classList.add('dark-mode');
    }
  }

  get showNavbar(): boolean {
    const hiddenRoutes = ['/auth/login', '/auth/register', '/auth/forgot-password', '/auth/reset-password', '/auth/2fa', '/landing', '/errors/'];
    return !hiddenRoutes.some(route => this.router.url.includes(route)) && this.router.url !== '/';
  }
}

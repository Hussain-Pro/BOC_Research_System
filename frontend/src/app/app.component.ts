import { Component, OnInit } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppShellComponent } from './components/app-shell/app-shell.component';
import { ToastContainerComponent } from './components/toast-container/toast-container.component';
import { ThemeService } from './services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, AppShellComponent, ToastContainerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'boc-research';

  constructor(
    public router: Router,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    // ThemeService constructor handles initialization from localStorage
  }

  /** Pages that use the full app-shell (sidebar + header) */
  get showAppShell(): boolean {
    const shellRoutes = [
      '/home', '/triage', '/research/', '/committee/',
      '/evaluator/', '/admin/', '/profile'
    ];
    return shellRoutes.some(route => this.router.url.startsWith(route));
  }

  /** Legacy: pages without any shell */
  get showLegacyToast(): boolean {
    return !this.showAppShell;
  }
}

import { Component, HostListener, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatMenuModule } from '@angular/material/menu';
import { filter, Subscription } from 'rxjs';
import { ThemeService } from '../../services/theme.service';
import { AuthService } from '../../services/auth.service';
import { CommandPaletteService } from '../../services/command-palette.service';
import { ToastSystemComponent } from '../toast-system/toast-system.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatMenuModule,
    ToastSystemComponent,
  ],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss']
})
export class AppShellComponent implements OnInit, OnDestroy {
  isSidebarCollapsed = false;
  isMobileSidebarOpen = false;
  isMobile = false;
  isScrolled = false;
  isDarkMode = false;
  unreadNotifications = 3;
  currentPageTitle = 'لوحة التحكم';

  get userName(): string {
    const user = this.authService.currentUser();
    return user?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'أحمد الخالدي';
  }

  get userInitials(): string {
    const name = this.userName;
    const parts = name.trim().split(' ');
    if (parts.length >= 2) {
      return parts[0][0] + parts[1][0];
    }
    return name.slice(0, 2);
  }

  private routerSub!: Subscription;
  private themeSub!: Subscription;

  constructor(
    private router: Router,
    private themeService: ThemeService,
    private authService: AuthService,
    private cmdPalette: CommandPaletteService
  ) {}

  ngOnInit(): void {
    this.checkMobile();

    this.themeSub = this.themeService.darkMode$.subscribe(dark => {
      this.isDarkMode = dark;
    });

    this.routerSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => {
        this.closeMobileSidebar();
        this.updatePageTitle();
      });

    // Restore sidebar state
    const saved = localStorage.getItem('boc_sidebar_collapsed');
    if (saved) this.isSidebarCollapsed = saved === 'true';

    // Initial title
    this.updatePageTitle();
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
    this.themeSub?.unsubscribe();
  }

  @HostListener('window:resize')
  checkMobile(): void {
    this.isMobile = window.innerWidth < 992;
    if (!this.isMobile) this.isMobileSidebarOpen = false;
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.isScrolled = window.scrollY > 10;
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    if ((event.ctrlKey || event.metaKey) && event.key === 'k') {
      event.preventDefault();
      this.openCommandPalette();
    }
    if (event.key === 'Escape') {
      this.closeMobileSidebar();
    }
  }

  toggleSidebar(): void {
    if (this.isMobile) {
      this.isMobileSidebarOpen = !this.isMobileSidebarOpen;
    } else {
      this.isSidebarCollapsed = !this.isSidebarCollapsed;
      localStorage.setItem('boc_sidebar_collapsed', String(this.isSidebarCollapsed));
    }
  }

  openMobileSidebar(): void {
    this.isMobileSidebarOpen = true;
  }

  closeMobileSidebar(): void {
    this.isMobileSidebarOpen = false;
  }

  toggleTheme(): void {
    this.themeService.toggleDarkMode();
  }

  toggleDir(): void {
    const html = document.documentElement;
    const newDir = html.dir === 'rtl' ? 'ltr' : 'rtl';
    html.dir = newDir;
    html.lang = newDir === 'rtl' ? 'ar' : 'en';
    localStorage.setItem('boc_direction', newDir);
  }

  openCommandPalette(): void {
    this.cmdPalette.open();
  }

  goToNotifications(): void {
    this.router.navigate(['/admin/notifications']);
  }

  logout(): void {
    this.authService.logout();
  }

  private updatePageTitle(): void {
    const url = this.router.url;
    const titleMap: Record<string, string> = {
      '/home': 'لوحة التحكم',
      '/research/submit': 'تقديم بحث',
      '/research/history': 'سجل الباحث',
      '/triage': 'لوحة الفرز',
      '/committee/workspace': 'مساحة اللجنة',
      '/committee/scheduler': 'جدول الاجتماعات',
      '/admin/analytics': 'لوحة المسؤول',
      '/profile': 'الملف الشخصي',
      '/admin/notifications': 'الإشعارات',
      '/admin/chat': 'المراسلة',
    };
    const matched = Object.keys(titleMap).find(k => url.startsWith(k));
    this.currentPageTitle = matched ? titleMap[matched] : 'النظام';
  }
}

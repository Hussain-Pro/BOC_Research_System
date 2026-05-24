import { Component, HostListener, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from '../../core/services/auth.service';
import { CommandPaletteService } from '../command-palette/command-palette.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  badge?: number;
}

interface NavSection {
  title: string;
  items: NavItem[];
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [], // RouterLink, RouterOutlet, NgFor, NgIf, MatMenuModule, TranslateModule, BreadcrumbComponent
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.scss']
})
export class AppShellComponent implements OnInit, OnDestroy {
  @ViewChild('sidebarRef') sidebarRef!: ElementRef;

  isSidebarCollapsed = false;
  isMobileSidebarOpen = false;
  isMobile = false;
  isScrolled = false;
  isDarkMode = false;
  unreadNotifications = 3;
  userName = 'أحمد الخالدي';
  userInitials = 'أح';

  breadcrumbs: { label: string; url?: string }[] = [
    { label: 'HOME.TITLE', url: '/' },
    { label: 'RESEARCH.TITLE' },
    { label: 'DASHBOARD.TITLE' }
  ];

  navSections: NavSection[] = [
    {
      title: 'NAV.MAIN',
      items: [
        { label: 'NAV.DASHBOARD', route: '/dashboard', icon: 'bi bi-grid' },
        { label: 'NAV.NOTIFICATIONS', route: '/notifications', icon: 'bi bi-bell', badge: 3 },
        { label: 'NAV.CHAT', route: '/chat', icon: 'bi bi-chat-dots' }
      ]
    },
    {
      title: 'NAV.RESEARCH',
      items: [
        { label: 'NAV.SUBMIT_RESEARCH', route: '/submit-research', icon: 'bi bi-file-earmark-text' },
        { label: 'NAV.RESEARCHER_HISTORY', route: '/researcher-history', icon: 'bi bi-clock-history' },
        { label: 'NAV.CORRECTIONS', route: '/research-corrections', icon: 'bi bi-clipboard-check' },
        { label: 'NAV.TIMELINE', route: '/research-timeline', icon: 'bi bi-diagram-3' }
      ]
    },
    {
      title: 'NAV.COMMITTEE',
      items: [
        { label: 'NAV.COMMITTEE_WORKSPACE', route: '/committee-workspace', icon: 'bi bi-people' },
        { label: 'NAV.MEETING_SCHEDULER', route: '/meeting-scheduler', icon: 'bi bi-calendar-week' },
        { label: 'NAV.MEETING_STUDIO', route: '/meeting-studio', icon: 'bi bi-camera-video' }
      ]
    },
    {
      title: 'NAV.ADMIN',
      items: [
        { label: 'NAV.ADMIN_PANEL', route: '/admin', icon: 'bi bi-shield-check' },
        { label: 'NAV.PROFILE', route: '/user-profile', icon: 'bi bi-person-gear' }
      ]
    }
  ];

  private routerSub!: Subscription;

  constructor(
    private router: Router,
    private translate: TranslateService,
    private themeService: ThemeService,
    private authService: AuthService,
    private cmdPalette: CommandPaletteService
  ) {}

  ngOnInit(): void {
    this.checkMobile();
    this.themeService.darkMode$.subscribe(dark => this.isDarkMode = dark);

    this.routerSub = this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => {
        this.closeMobileSidebar();
        this.updateBreadcrumbs();
      });

    // Restore sidebar state
    const saved = localStorage.getItem('boc_sidebar_collapsed');
    if (saved) this.isSidebarCollapsed = saved === 'true';
  }

  ngOnDestroy(): void {
    this.routerSub?.unsubscribe();
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

  isActive(route: string): boolean {
    return this.router.url.startsWith(route);
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
    this.translate.use(newDir === 'rtl' ? 'ar' : 'en');
  }

  openCommandPalette(): void {
    this.cmdPalette.open();
  }

  toggleNotifications(): void {
    // Navigate to notifications or open dropdown
    this.router.navigate(['/notifications']);
  }

  logout(): void {
    this.authService.logout();
  }

  private updateBreadcrumbs(): void {
    // Dynamic breadcrumb logic based on current route
    const url = this.router.url;
    // This would be populated by a breadcrumb service in production
  }
}

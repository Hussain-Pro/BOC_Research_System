import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatMenuModule } from '@angular/material/menu';
import { ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, TranslateModule, MatMenuModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  isDarkMode = false;
  currentLang = 'ar';

  constructor(
    private themeService: ThemeService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.themeService.darkMode$.subscribe(dark => this.isDarkMode = dark);
    this.currentLang = this.translate.currentLang || 'ar';
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

  setLang(lang: string): void {
    this.currentLang = lang;
    this.translate.use(lang);
    const html = document.documentElement;
    html.lang = lang;
    html.dir = lang === 'ar' ? 'rtl' : 'ltr';
    localStorage.setItem('boc_direction', html.dir);
  }
}

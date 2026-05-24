import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly STORAGE_KEY = 'boc_theme';
  private _darkMode = new BehaviorSubject<boolean>(false);

  darkMode$ = this._darkMode.asObservable();

  get isDarkMode(): boolean {
    return this._darkMode.value;
  }

  constructor() {
    // Initialize from localStorage
    const saved = localStorage.getItem(this.STORAGE_KEY);
    const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches;
    const isDark = saved ? saved === 'dark' : prefersDark;
    this.applyTheme(isDark);
  }

  toggleDarkMode(): void {
    this.applyTheme(!this._darkMode.value);
  }

  setDarkMode(dark: boolean): void {
    this.applyTheme(dark);
  }

  private applyTheme(dark: boolean): void {
    this._darkMode.next(dark);
    const html = document.documentElement;
    if (dark) {
      html.setAttribute('data-theme', 'dark');
      document.body.classList.add('dark-mode');
    } else {
      html.removeAttribute('data-theme');
      document.body.classList.remove('dark-mode');
    }
    localStorage.setItem(this.STORAGE_KEY, dark ? 'dark' : 'light');
  }
}

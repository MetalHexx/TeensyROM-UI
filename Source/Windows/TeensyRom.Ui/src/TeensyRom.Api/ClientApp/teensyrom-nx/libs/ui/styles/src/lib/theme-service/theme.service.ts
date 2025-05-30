import { inject, Injectable, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';

export type Theme = 'dark' | 'light';
const STORAGE_KEY = 'theme-preference';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly _document = inject(DOCUMENT);
  private readonly _currentTheme = signal<Theme>('light');
  readonly currentTheme = this._currentTheme.asReadonly();

  constructor() {
    this.loadThemeFromLocalStorage();
  }

  private loadThemeFromLocalStorage() {
    const saved = (localStorage.getItem(STORAGE_KEY) as Theme) ?? 'dark';
    this.setTheme(saved);
  }

  toggleTheme() {
    if (this._currentTheme() === 'dark') {
      this.setTheme('light');
    } else {
      this.setTheme('dark');
    }
  }

  setTheme(theme: Theme) {
    if (theme === 'dark') {
      this._document.documentElement.classList.add('dark-mode');
    } else {
      this._document.documentElement.classList.remove('dark-mode');
    }
    this._currentTheme.set(theme);
    localStorage.setItem(STORAGE_KEY, theme);
  }
}

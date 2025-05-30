import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';
import { DOCUMENT } from '@angular/common';

describe('ThemeService', () => {
  let service: ThemeService;
  let mockDocument: Document;
  let documentElement: HTMLElement;

  beforeEach(() => {
    // Reset localStorage between tests
    localStorage.clear();

    // Mock documentElement
    documentElement = document.createElement('html');
    mockDocument = {
      documentElement,
    } as unknown as Document;

    TestBed.configureTestingModule({
      providers: [{ provide: DOCUMENT, useValue: mockDocument }],
    });

    service = TestBed.inject(ThemeService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should default to dark theme when localStorage is empty', () => {
    expect(service.currentTheme()).toBe('dark');
    expect(documentElement.classList.contains('dark-mode')).toBe(true);
  });

  it('should load dark theme from localStorage', () => {
    localStorage.setItem('theme-preference', 'dark');
    // Re-instantiate service to trigger constructor again
    service = TestBed.inject(ThemeService);

    expect(service.currentTheme()).toBe('dark');
    expect(documentElement.classList.contains('dark-mode')).toBe(true);
  });

  it('should toggle from light to dark and update DOM and storage', () => {
    service.setTheme('light');
    service.toggleTheme();

    expect(service.currentTheme()).toBe('dark');
    expect(documentElement.classList.contains('dark-mode')).toBe(true);
    expect(localStorage.getItem('theme-preference')).toBe('dark');
  });

  it('should toggle from dark to light and update DOM and storage', () => {
    service.setTheme('dark');
    service.toggleTheme();

    expect(service.currentTheme()).toBe('light');
    expect(documentElement.classList.contains('dark-mode')).toBe(false);
    expect(localStorage.getItem('theme-preference')).toBe('light');
  });

  it('should set theme explicitly and reflect in DOM and storage', () => {
    service.setTheme('dark');
    expect(service.currentTheme()).toBe('dark');
    expect(documentElement.classList.contains('dark-mode')).toBe(true);
    expect(localStorage.getItem('theme-preference')).toBe('dark');

    service.setTheme('light');
    expect(service.currentTheme()).toBe('light');
    expect(documentElement.classList.contains('dark-mode')).toBe(false);
    expect(localStorage.getItem('theme-preference')).toBe('light');
  });
});

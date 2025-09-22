import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatChipsModule } from '@angular/material/chips';
import { DirectoryBreadcrumbComponent } from './directory-breadcrumb.component';

describe('DirectoryBreadcrumbComponent', () => {
  let component: DirectoryBreadcrumbComponent;
  let fixture: ComponentFixture<DirectoryBreadcrumbComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryBreadcrumbComponent, MatChipsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryBreadcrumbComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should require currentPath and storageType inputs', () => {
      fixture.componentRef.setInput('currentPath', '/test/path');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      expect(component.currentPath()).toBe('/test/path');
      expect(component.storageType()).toBe('SD Card');
    });
  });

  describe('Path Segment Calculation', () => {
    it('should handle root path', () => {
      fixture.componentRef.setInput('currentPath', '/');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([{ label: 'SD Card', path: '/' }]);
    });

    it('should handle empty path', () => {
      fixture.componentRef.setInput('currentPath', '');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([{ label: 'SD Card', path: '/' }]);
    });

    it('should handle single directory path', () => {
      fixture.componentRef.setInput('currentPath', '/games');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'games', path: '/games' },
      ]);
    });

    it('should handle nested directory path', () => {
      fixture.componentRef.setInput('currentPath', '/games/arcade/pacman');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'games', path: '/games' },
        { label: 'arcade', path: '/games/arcade' },
        { label: 'pacman', path: '/games/arcade/pacman' },
      ]);
    });

    it('should handle path with trailing slash', () => {
      fixture.componentRef.setInput('currentPath', '/games/arcade/');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'games', path: '/games' },
        { label: 'arcade', path: '/games/arcade' },
      ]);
    });

    it('should handle different storage type labels', () => {
      fixture.componentRef.setInput('currentPath', '/docs');
      fixture.componentRef.setInput('storageType', 'USB Drive');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'USB Drive', path: '/' },
        { label: 'docs', path: '/docs' },
      ]);
    });
  });

  describe('Navigation Event Handling', () => {
    it('should emit navigationRequested when onChipClick is called', () => {
      let emittedPath = '';
      component.navigationRequested.subscribe((path: string) => (emittedPath = path));

      component.onChipClick('/test/path');

      expect(emittedPath).toBe('/test/path');
    });

    it('should emit correct path for root click', () => {
      let emittedPath = '';
      component.navigationRequested.subscribe((path: string) => (emittedPath = path));

      component.onChipClick('/');

      expect(emittedPath).toBe('/');
    });
  });

  describe('Edge Cases', () => {
    it('should handle paths with multiple consecutive slashes', () => {
      fixture.componentRef.setInput('currentPath', '/games//arcade///pacman');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'games', path: '/games' },
        { label: 'arcade', path: '/games/arcade' },
        { label: 'pacman', path: '/games/arcade/pacman' },
      ]);
    });

    it('should handle path without leading slash', () => {
      fixture.componentRef.setInput('currentPath', 'games/arcade');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      const segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'games', path: '/games' },
        { label: 'arcade', path: '/games/arcade' },
      ]);
    });

    it('should update when inputs change', () => {
      fixture.componentRef.setInput('currentPath', '/initial');
      fixture.componentRef.setInput('storageType', 'SD Card');
      fixture.detectChanges();

      let segments = component.pathSegments();
      expect(segments).toHaveLength(2);

      fixture.componentRef.setInput('currentPath', '/new/path');
      fixture.detectChanges();

      segments = component.pathSegments();
      expect(segments).toEqual([
        { label: 'SD Card', path: '/' },
        { label: 'new', path: '/new' },
        { label: 'path', path: '/new/path' },
      ]);
    });
  });
});

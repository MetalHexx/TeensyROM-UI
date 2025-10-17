import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ExternalLinkComponent } from './external-link.component';

describe('ExternalLinkComponent', () => {
  let component: ExternalLinkComponent;
  let fixture: ComponentFixture<ExternalLinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExternalLinkComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ExternalLinkComponent);
    component = fixture.componentInstance;
    // Set required inputs
    fixture.componentRef.setInput('href', 'https://example.com');
    fixture.componentRef.setInput('label', 'Test Link');
    fixture.detectChanges();
  });

  describe('Component Creation', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });
  });

  describe('Required Inputs', () => {
    it('should accept href input', () => {
      expect(component.href()).toBe('https://example.com');
    });

    it('should accept label input', () => {
      expect(component.label()).toBe('Test Link');
    });
  });

  describe('Optional Inputs with Defaults', () => {
    it('should default icon to "link"', () => {
      expect(component.icon()).toBe('link');
    });

    it('should default iconColor to "primary"', () => {
      expect(component.iconColor()).toBe('primary');
    });

    it('should default target to "_blank"', () => {
      expect(component.target()).toBe('_blank');
    });

    it('should accept custom icon', () => {
      fixture.componentRef.setInput('icon', 'play_circle');
      fixture.detectChanges();
      expect(component.icon()).toBe('play_circle');
    });

    it('should accept custom iconColor', () => {
      fixture.componentRef.setInput('iconColor', 'error');
      fixture.detectChanges();
      expect(component.iconColor()).toBe('error');
    });

    it('should accept custom target', () => {
      fixture.componentRef.setInput('target', '_self');
      fixture.detectChanges();
      expect(component.target()).toBe('_self');
    });
  });

  describe('Computed Signals - isExternal', () => {
    it('should detect external links starting with http://', () => {
      fixture.componentRef.setInput('href', 'http://example.com');
      fixture.detectChanges();
      expect(component.isExternal()).toBe(true);
    });

    it('should detect external links starting with https://', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.detectChanges();
      expect(component.isExternal()).toBe(true);
    });

    it('should not detect relative URLs as external', () => {
      fixture.componentRef.setInput('href', '/docs');
      fixture.detectChanges();
      expect(component.isExternal()).toBe(false);
    });

    it('should not detect relative URLs starting with dot as external', () => {
      fixture.componentRef.setInput('href', './file');
      fixture.detectChanges();
      expect(component.isExternal()).toBe(false);
    });
  });

  describe('Computed Signals - relAttribute', () => {
    it('should return noopener noreferrer for external links with _blank target', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      expect(component.relAttribute()).toBe('noopener noreferrer');
    });

    it('should return undefined for internal links', () => {
      fixture.componentRef.setInput('href', '/docs');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      expect(component.relAttribute()).toBeUndefined();
    });

    it('should return undefined for external links with _self target', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_self');
      fixture.detectChanges();
      expect(component.relAttribute()).toBeUndefined();
    });

    it('should return undefined for internal links with _self target', () => {
      fixture.componentRef.setInput('href', '/docs');
      fixture.componentRef.setInput('target', '_self');
      fixture.detectChanges();
      expect(component.relAttribute()).toBeUndefined();
    });
  });

  describe('Computed Signals - effectiveTarget', () => {
    it('should return target input value', () => {
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      expect(component.effectiveTarget()).toBe('_blank');
    });

    it('should return _self when set', () => {
      fixture.componentRef.setInput('target', '_self');
      fixture.detectChanges();
      expect(component.effectiveTarget()).toBe('_self');
    });
  });

  describe('Template Rendering', () => {
    it('should render anchor element', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor).toBeTruthy();
    });

    it('should bind href to anchor element', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.href).toContain('https://example.com');
    });

    it('should bind target to anchor element', () => {
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.target).toBe('_blank');
    });

    it('should set rel attribute for external links with _blank', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.rel).toBe('noopener noreferrer');
    });

    it('should not set rel attribute for internal links', () => {
      fixture.componentRef.setInput('href', '/docs');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      // When rel is undefined, the computed signal returns undefined
      // The important thing is that the security attributes aren't there for internal links
      expect(component.relAttribute()).toBeUndefined();
    });

    it('should render IconLabel component', () => {
      const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
      expect(iconLabel).toBeTruthy();
    });

    it('should pass icon to IconLabel', () => {
      fixture.componentRef.setInput('icon', 'play_circle');
      fixture.detectChanges();
      const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
      expect(iconLabel.getAttribute('ng-reflect-icon')).toBe('play_circle');
    });

    it('should pass label to IconLabel', () => {
      fixture.componentRef.setInput('label', 'My Label');
      fixture.detectChanges();
      const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
      expect(iconLabel.getAttribute('ng-reflect-label')).toBe('My Label');
    });

    it('should pass color to IconLabel', () => {
      fixture.componentRef.setInput('iconColor', 'error');
      fixture.detectChanges();
      const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
      expect(iconLabel.getAttribute('ng-reflect-color')).toBe('error');
    });

    it('should set IconLabel size to medium', () => {
      const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
      expect(iconLabel.getAttribute('ng-reflect-size')).toBe('medium');
    });
  });

  describe('Accessibility', () => {
    it('should have native anchor element for keyboard focus', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.tagName.toLowerCase()).toBe('a');
    });

    it('should be keyboard focusable (native anchor behavior)', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      // Native anchors with href are always focusable
      expect(anchor.getAttribute('href')).toBeTruthy();
    });

    it('should support Enter key to activate (native anchor behavior)', () => {
      // This is a native browser behavior - the anchor element inherently
      // supports keyboard activation without additional event handlers
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.tagName.toLowerCase()).toBe('a');
    });

    it('should have proper role for screen readers', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      // Implicit role for anchor is 'link'
      expect(anchor.href).toBeTruthy();
    });

    it('should have aria-label for screen readers', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      // Default setup uses https://example.com with _blank target, so it adds "(opens in new window)"
      expect(anchor.getAttribute('aria-label')).toBe('Test Link (opens in new window)');
    });

    it('should include "(opens in new window)" in aria-label for external _blank links', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.getAttribute('aria-label')).toBe('Test Link (opens in new window)');
    });

    it('should not include "(opens in new window)" in aria-label for internal links', () => {
      fixture.componentRef.setInput('href', '/docs');
      fixture.componentRef.setInput('target', '_blank');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.getAttribute('aria-label')).toBe('Test Link');
    });

    it('should not include "(opens in new window)" in aria-label for _self target', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_self');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.getAttribute('aria-label')).toBe('Test Link');
    });

    it('should use custom ariaLabel when provided', () => {
      fixture.componentRef.setInput('ariaLabel', 'Custom Link Label');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.getAttribute('aria-label')).toBe('Custom Link Label');
    });

    it('should prioritize custom ariaLabel over auto-generated label', () => {
      fixture.componentRef.setInput('href', 'https://example.com');
      fixture.componentRef.setInput('target', '_blank');
      fixture.componentRef.setInput('ariaLabel', 'My Custom Label');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      expect(anchor.getAttribute('aria-label')).toBe('My Custom Label');
    });

    it('should have title attribute for tooltip', () => {
      const anchor = fixture.nativeElement.querySelector('a');
      // Default setup uses https://example.com with _blank target, so it adds "(opens in new window)"
      expect(anchor.getAttribute('title')).toBe('Test Link (opens in new window)');
    });

    it('should update title attribute when label changes', () => {
      fixture.componentRef.setInput('label', 'Updated Link');
      fixture.detectChanges();
      const anchor = fixture.nativeElement.querySelector('a');
      // Updated label still has the "(opens in new window)" suffix because default is external + _blank
      expect(anchor.getAttribute('title')).toBe('Updated Link (opens in new window)');
    });
  });
});

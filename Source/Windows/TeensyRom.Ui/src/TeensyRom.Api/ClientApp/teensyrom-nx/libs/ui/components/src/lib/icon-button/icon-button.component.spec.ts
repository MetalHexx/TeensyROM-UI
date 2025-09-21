import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import {
  IconButtonComponent,
  IconButtonSize,
  IconButtonVariant,
  IconButtonColor,
} from './icon-button.component';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('IconButtonComponent', () => {
  let component: IconButtonComponent;
  let fixture: ComponentFixture<IconButtonComponent>;
  let componentRef: ComponentRef<IconButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconButtonComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(IconButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    // Set required inputs
    componentRef.setInput('icon', 'power_settings_new');
    componentRef.setInput('ariaLabel', 'Power');
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should display the specified icon', () => {
    componentRef.setInput('icon', 'settings');
    componentRef.setInput('ariaLabel', 'Settings');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('mat-icon');
    expect(icon.textContent).toContain('settings');
  });

  it('should set aria-label correctly', () => {
    componentRef.setInput('icon', 'delete');
    componentRef.setInput('ariaLabel', 'Delete Item');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Delete Item');
  });

  it('should apply color classes correctly', () => {
    const colors: IconButtonColor[] = ['normal', 'highlight', 'success', 'error', 'dimmed'];

    colors.forEach((color) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('ariaLabel', 'Test');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      const icon = fixture.nativeElement.querySelector('mat-icon');
      expect(icon.classList.contains(color)).toBe(true);

      // Ensure other color classes are not present
      colors
        .filter((c) => c !== color)
        .forEach((otherColor) => {
          expect(icon.classList.contains(otherColor)).toBe(false);
        });
    });
  });

  it('should apply size classes correctly', () => {
    const sizes: IconButtonSize[] = ['small', 'medium', 'large'];

    sizes.forEach((size) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('ariaLabel', 'Test');
      componentRef.setInput('size', size);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');

      switch (size) {
        case 'small':
          expect(button.classList.contains('icon-button-small')).toBe(true);
          break;
        case 'medium':
          expect(button.classList.contains('icon-button-medium')).toBe(true);
          break;
        case 'large':
          expect(button.classList.contains('icon-button-large')).toBe(true);
          break;
      }
    });
  });

  it('should apply variant classes correctly', () => {
    const variants: IconButtonVariant[] = ['standard', 'rounded-primary', 'rounded-transparent'];

    variants.forEach((variant) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('ariaLabel', 'Test');
      componentRef.setInput('variant', variant);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');

      switch (variant) {
        case 'rounded-primary':
          expect(button.classList.contains('icon-button-rounded-primary')).toBe(true);
          break;
        case 'rounded-transparent':
          expect(button.classList.contains('icon-button-rounded-transparent')).toBe(true);
          break;
        case 'standard':
          // Standard has no additional classes
          expect(button.classList.contains('icon-button-rounded-primary')).toBe(false);
          expect(button.classList.contains('icon-button-rounded-transparent')).toBe(false);
          break;
      }
    });
  });

  it('should handle disabled state correctly', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('ariaLabel', 'Test');
    componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should emit buttonClick event when clicked and not disabled', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('ariaLabel', 'Test');
    componentRef.setInput('disabled', false);
    fixture.detectChanges();

    const clickSpy = vi.spyOn(component.buttonClick, 'emit');

    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickSpy).toHaveBeenCalled();
  });

  it('should not emit buttonClick event when disabled', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('ariaLabel', 'Test');
    componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const clickSpy = vi.spyOn(component.buttonClick, 'emit');

    // Try to trigger click via component method
    component.onButtonClick();

    expect(clickSpy).not.toHaveBeenCalled();
  });

  it('should combine multiple classes correctly', () => {
    componentRef.setInput('icon', 'power_settings_new');
    componentRef.setInput('ariaLabel', 'Power');
    componentRef.setInput('size', 'medium');
    componentRef.setInput('variant', 'rounded-primary');
    componentRef.setInput('color', 'highlight');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    const icon = fixture.nativeElement.querySelector('mat-icon');

    expect(button.classList.contains('icon-button-medium')).toBe(true);
    expect(button.classList.contains('icon-button-rounded-primary')).toBe(true);
    expect(icon.classList.contains('highlight')).toBe(true);
  });

  it('should have default values for optional properties', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('ariaLabel', 'Test');
    fixture.detectChanges();

    expect(component.color()).toBe('normal');
    expect(component.size()).toBe('medium');
    expect(component.variant()).toBe('standard');
    expect(component.disabled()).toBe(false);
  });

  it('should be accessible with proper ARIA attributes', () => {
    componentRef.setInput('icon', 'accessibility');
    componentRef.setInput('ariaLabel', 'Accessibility Test Button');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Accessibility Test Button');
    expect(button.tagName.toLowerCase()).toBe('button');
  });
});

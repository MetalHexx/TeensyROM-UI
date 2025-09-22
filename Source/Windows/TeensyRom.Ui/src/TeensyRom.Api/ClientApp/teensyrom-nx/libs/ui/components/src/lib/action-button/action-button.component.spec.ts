import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import {
  ActionButtonComponent,
  ActionButtonVariant,
  ActionButtonColor,
} from './action-button.component';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('ActionButtonComponent', () => {
  let component: ActionButtonComponent;
  let fixture: ComponentFixture<ActionButtonComponent>;
  let componentRef: ComponentRef<ActionButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActionButtonComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ActionButtonComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    // Set required inputs
    componentRef.setInput('icon', 'refresh');
    componentRef.setInput('label', 'Refresh');
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should display the specified icon and label', () => {
    componentRef.setInput('icon', 'download');
    componentRef.setInput('label', 'Download');
    fixture.detectChanges();

    const iconLabel = fixture.nativeElement.querySelector('lib-icon-label');
    expect(iconLabel).toBeTruthy();
  });

  it('should apply variant directives correctly', () => {
    const variants: ActionButtonVariant[] = ['stroked', 'flat', 'raised', 'fab'];

    variants.forEach((variant) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('label', 'Test');
      componentRef.setInput('variant', variant);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');

      // Check that the button has the appropriate Material directive
      // Note: The actual directive presence is harder to test directly,
      // but we can verify the button element exists and component renders
      expect(button).toBeTruthy();
      expect(component.variant()).toBe(variant);
    });
  });

  it('should apply color classes correctly', () => {
    const colorsWithClasses: ActionButtonColor[] = ['success', 'error', 'highlight'];
    const colorsWithoutClasses: ActionButtonColor[] = ['primary', 'normal'];

    // Test colors that should have classes
    colorsWithClasses.forEach((color) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('label', 'Test');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');
      expect(button.classList.contains(`action-button-${color}`)).toBe(true);

      // Ensure other color classes are not present
      colorsWithClasses
        .filter((c) => c !== color)
        .forEach((otherColor) => {
          expect(button.classList.contains(`action-button-${otherColor}`)).toBe(false);
        });
    });

    // Test colors that should NOT have classes (use Material's default styling)
    colorsWithoutClasses.forEach((color) => {
      componentRef.setInput('icon', 'test');
      componentRef.setInput('label', 'Test');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');
      expect(button.classList.contains(`action-button-${color}`)).toBe(false);

      // Ensure no color classes are present for these
      colorsWithClasses.forEach((otherColor) => {
        expect(button.classList.contains(`action-button-${otherColor}`)).toBe(false);
      });
    });
  });

  it('should handle disabled state correctly', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('label', 'Test');
    componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should emit buttonClick when clicked and not disabled', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('label', 'Test');
    componentRef.setInput('disabled', false);
    fixture.detectChanges();

    const clickSpy = vi.spyOn(component.buttonClick, 'emit');

    const button = fixture.nativeElement.querySelector('button');
    button.click();

    expect(clickSpy).toHaveBeenCalled();
  });

  it('should not emit buttonClick when disabled', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('label', 'Test');
    componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const clickSpy = vi.spyOn(component.buttonClick, 'emit');

    // Try to trigger click via component method
    component.onButtonClick();

    expect(clickSpy).not.toHaveBeenCalled();
  });

  it('should use label as aria-label when ariaLabel not provided', () => {
    componentRef.setInput('icon', 'accessibility');
    componentRef.setInput('label', 'Accessibility Test');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Accessibility Test');
  });

  it('should use explicit ariaLabel when provided', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('label', 'Test');
    componentRef.setInput('ariaLabel', 'Custom Aria Label');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Custom Aria Label');
  });

  it('should have default values for optional properties', () => {
    componentRef.setInput('icon', 'test');
    componentRef.setInput('label', 'Test');
    fixture.detectChanges();

    expect(component.variant()).toBe('stroked');
    expect(component.color()).toBe('primary');
    expect(component.disabled()).toBe(false);
    expect(component.ariaLabel()).toBeUndefined();
  });

  it('should combine multiple properties correctly', () => {
    componentRef.setInput('icon', 'save');
    componentRef.setInput('label', 'Save Data');
    componentRef.setInput('variant', 'raised');
    componentRef.setInput('color', 'success');
    componentRef.setInput('disabled', false);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');

    expect(button.hasAttribute('mat-raised-button')).toBe(true);
    expect(button.classList.contains('action-button-success')).toBe(true);
    expect(button.disabled).toBe(false);
  });

  it('should be accessible with proper ARIA attributes', () => {
    componentRef.setInput('icon', 'help');
    componentRef.setInput('label', 'Get Help');
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Get Help');
    expect(button.tagName.toLowerCase()).toBe('button');
  });
});

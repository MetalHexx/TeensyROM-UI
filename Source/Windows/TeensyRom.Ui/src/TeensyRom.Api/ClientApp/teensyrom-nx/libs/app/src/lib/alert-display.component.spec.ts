import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlertDisplayComponent } from './alert-display.component';
import { AlertMessage, AlertSeverity, AlertPosition } from '@teensyrom-nx/domain';
import { By } from '@angular/platform-browser';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('AlertDisplayComponent', () => {
  let component: AlertDisplayComponent;
  let fixture: ComponentFixture<AlertDisplayComponent>;

  const mockAlert: AlertMessage = {
    id: 'test-id-123',
    message: 'Test error message',
    severity: AlertSeverity.Error,
    position: AlertPosition.BottomRight,
    autoDismissMs: 3000,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AlertDisplayComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertDisplayComponent);
    component = fixture.componentInstance;
  });

  it('should render alert message text', () => {
    fixture.componentRef.setInput('alert', mockAlert);
    fixture.detectChanges();

    const messageElement = fixture.debugElement.query(By.css('.alert-message'));
    expect(messageElement.nativeElement.textContent).toContain('Test error message');
  });

  it('should display correct icon for error severity', () => {
    fixture.componentRef.setInput('alert', mockAlert);
    fixture.detectChanges();

    expect(component.iconName()).toBe('error');
    const iconElement = fixture.debugElement.query(By.css('mat-icon:first-child'));
    expect(iconElement.nativeElement.textContent).toContain('error');
  });

  it('should display check_circle icon for success severity', () => {
    const successAlert: AlertMessage = {
      ...mockAlert,
      severity: AlertSeverity.Success,
    };
    fixture.componentRef.setInput('alert', successAlert);
    fixture.detectChanges();

    expect(component.iconName()).toBe('check_circle');
  });

  it('should display warning icon for warning severity', () => {
    const warningAlert: AlertMessage = {
      ...mockAlert,
      severity: AlertSeverity.Warning,
    };
    fixture.componentRef.setInput('alert', warningAlert);
    fixture.detectChanges();

    expect(component.iconName()).toBe('warning');
  });

  it('should display info icon for info severity', () => {
    const infoAlert: AlertMessage = {
      ...mockAlert,
      severity: AlertSeverity.Info,
    };
    fixture.componentRef.setInput('alert', infoAlert);
    fixture.detectChanges();

    expect(component.iconName()).toBe('info');
  });

  it('should emit dismissed event with alert id when dismiss button clicked', () => {
    fixture.componentRef.setInput('alert', mockAlert);
    fixture.detectChanges();

    const dismissedSpy = vi.spyOn(component.dismissed, 'emit');

    const dismissButton = fixture.debugElement.query(By.css('button'));
    dismissButton.nativeElement.click();

    expect(dismissedSpy).toHaveBeenCalledWith('test-id-123');
  });

  it('should render dismiss button with proper aria-label', () => {
    fixture.componentRef.setInput('alert', mockAlert);
    fixture.detectChanges();

    const dismissButton = fixture.debugElement.query(By.css('button'));
    expect(dismissButton.nativeElement.getAttribute('aria-label')).toBe('Dismiss alert');
  });

  it('should render all visual elements', () => {
    fixture.componentRef.setInput('alert', mockAlert);
    fixture.detectChanges();

    const icon = fixture.debugElement.query(By.css('.alert-icon'));
    const message = fixture.debugElement.query(By.css('.alert-message'));
    const button = fixture.debugElement.query(By.css('button'));

    expect(icon).toBeTruthy();
    expect(message).toBeTruthy();
    expect(button).toBeTruthy();
  });
});

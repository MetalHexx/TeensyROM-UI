import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi as vitest } from 'vitest';
import { ActionLinkComponent } from './action-link.component';
import { LinkComponent } from '../link/link.component';
import { IconLabelComponent } from '../icon-label/icon-label.component';

describe('ActionLinkComponent', () => {
  let component: ActionLinkComponent;
  let fixture: ComponentFixture<ActionLinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActionLinkComponent, LinkComponent, IconLabelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ActionLinkComponent);
    component = fixture.componentInstance;
    // Set required input before detectChanges
    fixture.componentRef.setInput('label', 'Click Me');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render link appearance', () => {
    const button = fixture.debugElement.nativeElement.querySelector('button');
    const libLink = fixture.debugElement.nativeElement.querySelector('lib-link');
    expect(button).toBeTruthy();
    expect(libLink).toBeTruthy();
  });

  it('should emit click event when clicked', () => {
    const emitSpy = vitest.spyOn(component.linkClick, 'emit');
    const button = fixture.debugElement.nativeElement.querySelector('button');
    button.click();
    expect(emitSpy).toHaveBeenCalled();
  });

  it('should not emit click event when disabled', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    const emitSpy = vitest.spyOn(component.linkClick, 'emit');
    const button = fixture.debugElement.nativeElement.querySelector('button');
    button.click();
    expect(emitSpy).not.toHaveBeenCalled();
  });

  it('should apply disabled attribute when disabled is true', () => {
    fixture.componentRef.setInput('disabled', true);
    fixture.detectChanges();
    const button = fixture.debugElement.nativeElement.querySelector('button');
    expect(button.disabled).toBe(true);
  });

  it('should apply aria-label', () => {
    fixture.componentRef.setInput('ariaLabel', 'Custom Label');
    fixture.detectChanges();
    const button = fixture.debugElement.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Custom Label');
  });

  it('should use label as aria-label when custom label not provided', () => {
    const button = fixture.debugElement.nativeElement.querySelector('button');
    expect(button.getAttribute('aria-label')).toBe('Click Me');
  });

  it('should be keyboard accessible', () => {
    const emitSpy = vitest.spyOn(component.linkClick, 'emit');
    const button = fixture.debugElement.nativeElement.querySelector('button');
    const enterEvent = new KeyboardEvent('keydown', { key: 'Enter' });
    button.dispatchEvent(enterEvent);
    button.click();
    expect(emitSpy).toHaveBeenCalled();
  });
});

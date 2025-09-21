import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { InputFieldComponent } from './input-field.component';
import { ComponentRef } from '@angular/core';
import { vi } from 'vitest';

describe('InputFieldComponent', () => {
  let component: InputFieldComponent;
  let fixture: ComponentFixture<InputFieldComponent>;
  let componentRef: ComponentRef<InputFieldComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputFieldComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(InputFieldComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    // Set required inputs
    componentRef.setInput('label', 'Test Label');
    componentRef.setInput('placeholder', 'Test Placeholder');
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should display label and placeholder', () => {
    componentRef.setInput('label', 'Username');
    componentRef.setInput('placeholder', 'Enter your username');
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.querySelector('mat-label').textContent).toContain('Username');
    expect(compiled.querySelector('input').placeholder).toBe('Enter your username');
  });

  it('should display prefix icon when provided', () => {
    componentRef.setInput('label', 'Email');
    componentRef.setInput('placeholder', 'Enter email');
    componentRef.setInput('prefixIcon', 'email');
    fixture.detectChanges();

    const prefixIcon = fixture.nativeElement.querySelector('mat-icon[matPrefix]');
    expect(prefixIcon).toBeTruthy();
    expect(prefixIcon.textContent).toContain('email');
  });

  it('should display suffix icon when provided', () => {
    componentRef.setInput('label', 'Search');
    componentRef.setInput('placeholder', 'Search files');
    componentRef.setInput('suffixIcon', 'search');
    fixture.detectChanges();

    const suffixIcon = fixture.nativeElement.querySelector('mat-icon[matSuffix]');
    expect(suffixIcon).toBeTruthy();
    expect(suffixIcon.textContent).toContain('search');
  });

  it('should not display icons when not provided', () => {
    componentRef.setInput('label', 'Simple Input');
    componentRef.setInput('placeholder', 'Enter text');
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('mat-icon[matPrefix]')).toBeFalsy();
    expect(fixture.nativeElement.querySelector('mat-icon[matSuffix]')).toBeFalsy();
  });

  it('should handle input type correctly', () => {
    componentRef.setInput('label', 'Password');
    componentRef.setInput('placeholder', 'Enter password');
    componentRef.setInput('inputType', 'password');
    fixture.detectChanges();

    const input = fixture.nativeElement.querySelector('input');
    expect(input.type).toBe('password');
  });

  it('should handle disabled state', () => {
    componentRef.setInput('label', 'Disabled Input');
    componentRef.setInput('placeholder', 'This is disabled');
    componentRef.setInput('disabled', true);
    fixture.detectChanges();

    const input = fixture.nativeElement.querySelector('input');
    expect(input.disabled).toBe(true);
  });

  it('should emit inputFocus and inputBlur events', () => {
    componentRef.setInput('label', 'Test Input');
    componentRef.setInput('placeholder', 'Test');
    fixture.detectChanges();

    const focusSpy = vi.spyOn(component.inputFocus, 'emit');
    const blurSpy = vi.spyOn(component.inputBlur, 'emit');

    const input = fixture.nativeElement.querySelector('input');
    input.dispatchEvent(new Event('focus'));
    input.dispatchEvent(new Event('blur'));

    expect(focusSpy).toHaveBeenCalled();
    expect(blurSpy).toHaveBeenCalled();
  });

  it('should emit valueChange event on input', () => {
    componentRef.setInput('label', 'Test Input');
    componentRef.setInput('placeholder', 'Test');
    fixture.detectChanges();

    const valueChangeSpy = vi.spyOn(component.valueChange, 'emit');

    const input = fixture.nativeElement.querySelector('input');
    input.value = 'test value';
    input.dispatchEvent(new Event('input'));

    expect(valueChangeSpy).toHaveBeenCalledWith('test value');
  });

  it('should handle multiple input changes', () => {
    componentRef.setInput('label', 'Search');
    componentRef.setInput('placeholder', 'Type to search...');
    fixture.detectChanges();

    const valueChangeSpy = vi.spyOn(component.valueChange, 'emit');
    const input = fixture.nativeElement.querySelector('input');

    // Simulate typing "hello"
    const values = ['h', 'he', 'hel', 'hell', 'hello'];
    values.forEach((value) => {
      input.value = value;
      input.dispatchEvent(new Event('input'));
    });

    expect(valueChangeSpy).toHaveBeenCalledTimes(5);
    expect(valueChangeSpy).toHaveBeenLastCalledWith('hello');
  });
});

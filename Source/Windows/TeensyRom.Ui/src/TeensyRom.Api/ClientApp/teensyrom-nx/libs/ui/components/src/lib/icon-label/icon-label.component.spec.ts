import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { IconLabelComponent } from './icon-label.component';
import { ComponentRef } from '@angular/core';
import { StyledIconColor, StyledIconSize } from '../styled-icon/styled-icon.component';

describe('IconLabelComponent', () => {
  let component: IconLabelComponent;
  let fixture: ComponentFixture<IconLabelComponent>;
  let componentRef: ComponentRef<IconLabelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconLabelComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(IconLabelComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Test Label');
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should display icon and label', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'My Folder');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('lib-styled-icon');
    const label = fixture.nativeElement.querySelector('.icon-label-text');

    expect(icon).toBeTruthy();
    expect(label.textContent).toContain('My Folder');
  });

  it('should use default color and size when not specified', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Test');
    fixture.detectChanges();

    expect(component.color()).toBe('normal');
    expect(component.size()).toBe('medium');
  });

  it('should apply custom color', () => {
    const colors: StyledIconColor[] = ['primary', 'highlight', 'success', 'error', 'directory'];

    colors.forEach((color) => {
      componentRef.setInput('icon', 'folder');
      componentRef.setInput('label', 'Test');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      expect(component.color()).toBe(color);
    });
  });

  it('should apply custom size', () => {
    const sizes: StyledIconSize[] = ['small', 'medium', 'large'];

    sizes.forEach((size) => {
      componentRef.setInput('icon', 'folder');
      componentRef.setInput('label', 'Test');
      componentRef.setInput('size', size);
      fixture.detectChanges();

      expect(component.size()).toBe(size);
    });
  });

  it('should truncate text by default', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Very Long Label Text That Should Be Truncated');
    fixture.detectChanges();

    const labelElement = fixture.nativeElement.querySelector('.icon-label-text');
    expect(labelElement.classList.contains('truncate')).toBe(true);
    expect(component.truncate()).toBe(true);
  });

  it('should not truncate text when truncate is false', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Long Label Text');
    componentRef.setInput('truncate', false);
    fixture.detectChanges();

    const labelElement = fixture.nativeElement.querySelector('.icon-label-text');
    expect(labelElement.classList.contains('truncate')).toBe(false);
    expect(component.truncate()).toBe(false);
  });

  it('should set title attribute on label', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Hover Text');
    fixture.detectChanges();

    const labelElement = fixture.nativeElement.querySelector('.icon-label-text');
    expect(labelElement.getAttribute('title')).toBe('Hover Text');
  });

  it('should work with all prop combinations', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('label', 'Complete Test');
    componentRef.setInput('color', 'directory');
    componentRef.setInput('size', 'large');
    componentRef.setInput('truncate', false);
    fixture.detectChanges();

    expect(component.icon()).toBe('folder');
    expect(component.label()).toBe('Complete Test');
    expect(component.color()).toBe('directory');
    expect(component.size()).toBe('large');
    expect(component.truncate()).toBe(false);
  });

  it('should maintain backward compatibility with minimal props', () => {
    componentRef.setInput('icon', 'info');
    componentRef.setInput('label', 'Simple');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('lib-styled-icon');
    const label = fixture.nativeElement.querySelector('.icon-label-text');

    expect(icon).toBeTruthy();
    expect(label.textContent).toContain('Simple');
    expect(label.classList.contains('truncate')).toBe(true);
  });
});

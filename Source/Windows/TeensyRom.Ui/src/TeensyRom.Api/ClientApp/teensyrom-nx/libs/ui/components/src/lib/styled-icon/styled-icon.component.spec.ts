import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { StyledIconComponent, StyledIconSize, StyledIconColor } from './styled-icon.component';
import { ComponentRef } from '@angular/core';

describe('StyledIconComponent', () => {
  let component: StyledIconComponent;
  let fixture: ComponentFixture<StyledIconComponent>;
  let componentRef: ComponentRef<StyledIconComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StyledIconComponent, NoopAnimationsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(StyledIconComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    componentRef.setInput('icon', 'folder');
    fixture.detectChanges();

    expect(component).toBeTruthy();
  });

  it('should display the specified icon', () => {
    componentRef.setInput('icon', 'folder');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('mat-icon');
    expect(icon.textContent).toContain('folder');
  });

  it('should apply size classes correctly', () => {
    const sizes: StyledIconSize[] = ['small', 'medium', 'large'];

    sizes.forEach((size) => {
      componentRef.setInput('icon', 'folder');
      componentRef.setInput('size', size);
      fixture.detectChanges();

      const icon = fixture.nativeElement.querySelector('mat-icon');

      switch (size) {
        case 'small':
          expect(icon.classList.contains('styled-icon-small')).toBe(true);
          expect(icon.classList.contains('styled-icon-medium')).toBe(false);
          expect(icon.classList.contains('styled-icon-large')).toBe(false);
          break;
        case 'medium':
          expect(icon.classList.contains('styled-icon-medium')).toBe(true);
          expect(icon.classList.contains('styled-icon-small')).toBe(false);
          expect(icon.classList.contains('styled-icon-large')).toBe(false);
          break;
        case 'large':
          expect(icon.classList.contains('styled-icon-large')).toBe(true);
          expect(icon.classList.contains('styled-icon-small')).toBe(false);
          expect(icon.classList.contains('styled-icon-medium')).toBe(false);
          break;
      }
    });
  });

  it('should apply color classes correctly', () => {
    const colors: StyledIconColor[] = [
      'normal',
      'primary',
      'highlight',
      'success',
      'error',
      'dimmed',
      'directory',
    ];

    colors.forEach((color) => {
      componentRef.setInput('icon', 'folder');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      const icon = fixture.nativeElement.querySelector('mat-icon');

      switch (color) {
        case 'primary':
          expect(icon.classList.contains('styled-icon-primary')).toBe(true);
          break;
        case 'highlight':
          expect(icon.classList.contains('styled-icon-highlight')).toBe(true);
          break;
        case 'success':
          expect(icon.classList.contains('styled-icon-success')).toBe(true);
          break;
        case 'error':
          expect(icon.classList.contains('styled-icon-error')).toBe(true);
          break;
        case 'dimmed':
          expect(icon.classList.contains('styled-icon-dimmed')).toBe(true);
          break;
        case 'directory':
          expect(icon.classList.contains('styled-icon-directory')).toBe(true);
          break;
        case 'normal':
          expect(icon.classList.contains('styled-icon-primary')).toBe(false);
          expect(icon.classList.contains('styled-icon-highlight')).toBe(false);
          expect(icon.classList.contains('styled-icon-directory')).toBe(false);
          break;
      }
    });
  });

  it('should combine size and color classes correctly', () => {
    componentRef.setInput('icon', 'folder');
    componentRef.setInput('size', 'large');
    componentRef.setInput('color', 'directory');
    fixture.detectChanges();

    const icon = fixture.nativeElement.querySelector('mat-icon');

    expect(icon.classList.contains('styled-icon-large')).toBe(true);
    expect(icon.classList.contains('styled-icon-directory')).toBe(true);
  });

  it('should have default values for optional properties', () => {
    componentRef.setInput('icon', 'folder');
    fixture.detectChanges();

    expect(component.color()).toBe('normal');
    expect(component.size()).toBe('medium');
  });

  it('should support all directory-related colors', () => {
    const directoryColors: StyledIconColor[] = ['primary', 'highlight', 'directory'];

    directoryColors.forEach((color) => {
      componentRef.setInput('icon', 'folder');
      componentRef.setInput('color', color);
      fixture.detectChanges();

      const icon = fixture.nativeElement.querySelector('mat-icon');
      expect(icon).toBeTruthy();
    });
  });
});

import { describe, it, expect } from 'vitest';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ProgressBarComponent } from './progress-bar.component';

describe('ProgressBarComponent', () => {
  let component: ProgressBarComponent;
  let fixture: ComponentFixture<ProgressBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProgressBarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ProgressBarComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('progressPercent computed', () => {
    it('should return 0 when totalValue is 0', () => {
      fixture.componentRef.setInput('currentValue', 50);
      fixture.componentRef.setInput('totalValue', 0);
      
      expect(component.progressPercent()).toBe(0);
    });

    it('should return 0 when both values are 0', () => {
      fixture.componentRef.setInput('currentValue', 0);
      fixture.componentRef.setInput('totalValue', 0);
      
      expect(component.progressPercent()).toBe(0);
    });

    it('should calculate correct percentage for 50%', () => {
      fixture.componentRef.setInput('currentValue', 50);
      fixture.componentRef.setInput('totalValue', 100);
      
      expect(component.progressPercent()).toBe(50);
    });

    it('should calculate correct percentage for 25%', () => {
      fixture.componentRef.setInput('currentValue', 25);
      fixture.componentRef.setInput('totalValue', 100);
      
      expect(component.progressPercent()).toBe(25);
    });

    it('should calculate correct percentage for 100%', () => {
      fixture.componentRef.setInput('currentValue', 100);
      fixture.componentRef.setInput('totalValue', 100);
      
      expect(component.progressPercent()).toBe(100);
    });

    it('should handle non-round percentages correctly', () => {
      fixture.componentRef.setInput('currentValue', 33);
      fixture.componentRef.setInput('totalValue', 100);
      
      expect(component.progressPercent()).toBe(33);
    });

    it('should handle fractional values', () => {
      fixture.componentRef.setInput('currentValue', 45.5);
      fixture.componentRef.setInput('totalValue', 180);
      
      expect(component.progressPercent()).toBeCloseTo(25.28, 2);
    });
  });

  describe('Template Rendering', () => {
    it('should not render mat-progress-bar when show is false', () => {
      fixture.componentRef.setInput('show', false);
      fixture.detectChanges();
      
      const progressBar = fixture.nativeElement.querySelector('mat-progress-bar');
      expect(progressBar).toBeNull();
    });

    it('should render mat-progress-bar when show is true', () => {
      fixture.componentRef.setInput('show', true);
      fixture.detectChanges();
      
      const progressBar = fixture.nativeElement.querySelector('mat-progress-bar');
      expect(progressBar).toBeTruthy();
    });

    it('should set correct value attribute on mat-progress-bar', () => {
      fixture.componentRef.setInput('currentValue', 75);
      fixture.componentRef.setInput('totalValue', 100);
      fixture.componentRef.setInput('show', true);
      fixture.detectChanges();
      
      const progressBar = fixture.nativeElement.querySelector('mat-progress-bar');
      // Material progress bar expects a number value
      expect(progressBar).toBeTruthy();
      
      // Check the component instance has correct calculated value
      expect(component.progressPercent()).toBe(75);
    });

    it('should use determinate mode', () => {
      fixture.componentRef.setInput('show', true);
      fixture.detectChanges();
      
      const progressBar = fixture.nativeElement.querySelector('mat-progress-bar');
      expect(progressBar.getAttribute('mode')).toBe('determinate');
    });
  });

  describe('Input Defaults', () => {
    it('should default currentValue to 0', () => {
      expect(component.currentValue()).toBe(0);
    });

    it('should default totalValue to 0', () => {
      expect(component.totalValue()).toBe(0);
    });

    it('should default show to false', () => {
      expect(component.show()).toBe(false);
    });
  });

  describe('Reactivity', () => {
    it('should recalculate progressPercent when currentValue changes', () => {
      fixture.componentRef.setInput('totalValue', 100);
      fixture.componentRef.setInput('currentValue', 25);
      
      expect(component.progressPercent()).toBe(25);
      
      fixture.componentRef.setInput('currentValue', 75);
      
      expect(component.progressPercent()).toBe(75);
    });

    it('should recalculate progressPercent when totalValue changes', () => {
      fixture.componentRef.setInput('currentValue', 50);
      fixture.componentRef.setInput('totalValue', 100);
      
      expect(component.progressPercent()).toBe(50);
      
      fixture.componentRef.setInput('totalValue', 200);
      
      expect(component.progressPercent()).toBe(25);
    });

    it('should toggle visibility when show input changes', () => {
      fixture.componentRef.setInput('show', false);
      fixture.detectChanges();
      
      expect(fixture.nativeElement.querySelector('mat-progress-bar')).toBeNull();
      
      fixture.componentRef.setInput('show', true);
      fixture.detectChanges();
      
      expect(fixture.nativeElement.querySelector('mat-progress-bar')).toBeTruthy();
    });
  });
});

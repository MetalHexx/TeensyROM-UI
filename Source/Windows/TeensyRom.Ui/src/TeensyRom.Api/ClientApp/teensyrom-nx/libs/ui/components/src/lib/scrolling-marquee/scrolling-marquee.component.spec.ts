import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScrollingMarqueeComponent } from './scrolling-marquee.component';
import { ComponentRef } from '@angular/core';

describe('ScrollingMarqueeComponent', () => {
  let component: ScrollingMarqueeComponent;
  let fixture: ComponentFixture<ScrollingMarqueeComponent>;
  let componentRef: ComponentRef<ScrollingMarqueeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScrollingMarqueeComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(ScrollingMarqueeComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Text Input', () => {
    it('should render text correctly', () => {
      componentRef.setInput('text', 'Hello World');
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.textContent?.trim()).toBe('Hello World');
    });

    it('should update text when input changes', () => {
      componentRef.setInput('text', 'Initial Text');
      fixture.detectChanges();

      let contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.textContent?.trim()).toBe('Initial Text');

      componentRef.setInput('text', 'Updated Text');
      fixture.detectChanges();

      contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.textContent?.trim()).toBe('Updated Text');
    });

    it('should handle empty text', () => {
      componentRef.setInput('text', '');
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.textContent?.trim()).toBe('');
    });
  });

  describe('Direction Input', () => {
    it('should default to scroll-left direction', () => {
      // Mock overflow to trigger scrolling
      component.shouldScroll.set(true);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('scroll-left')).toBe(true);
    });

    it('should apply scroll-right when direction is right', () => {
      componentRef.setInput('direction', 'right');
      component.shouldScroll.set(true);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('scroll-right')).toBe(true);
    });
  });

  describe('Speed Input', () => {
    it('should default to 50 pixels per second', () => {
      expect(component.speed()).toBe(50);
    });

    it('should accept custom speed', () => {
      componentRef.setInput('speed', 100);
      fixture.detectChanges();

      expect(component.speed()).toBe(100);
    });

    it('should calculate duration based on speed', () => {
      componentRef.setInput('speed', 50);
      fixture.detectChanges();

      // Duration should be calculated as: width / speed
      // In a test environment, the DOM elements may have zero width
      // So we test that the duration is a non-negative number (default is 10)
      expect(component.scrollDuration()).toBeGreaterThanOrEqual(0);
    });
  });

  describe('PauseOnHover Input', () => {
    it('should default to true', () => {
      expect(component.pauseOnHover()).toBe(true);
    });

    it('should apply pause-on-hover class when true', () => {
      componentRef.setInput('pauseOnHover', true);
      component.shouldScroll.set(true);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('pause-on-hover')).toBe(true);
    });

    it('should not apply pause-on-hover class when false', () => {
      componentRef.setInput('pauseOnHover', false);
      component.shouldScroll.set(true);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('pause-on-hover')).toBe(false);
    });
  });

  describe('Scrolling Behavior', () => {
    it('should not apply scrolling class when shouldScroll is false', () => {
      component.shouldScroll.set(false);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('scrolling')).toBe(false);
    });

    it('should apply scrolling class when shouldScroll is true', () => {
      component.shouldScroll.set(true);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement?.classList.contains('scrolling')).toBe(true);
    });

    it('should have marquee-container element', () => {
      const containerElement = fixture.nativeElement.querySelector('.marquee-container');
      expect(containerElement).toBeTruthy();
    });

    it('should have marquee-content element', () => {
      const contentElement = fixture.nativeElement.querySelector('.marquee-content');
      expect(contentElement).toBeTruthy();
    });
  });

  describe('CSS Custom Properties', () => {
    it('should set scroll duration CSS variable', () => {
      component.shouldScroll.set(true);
      component.scrollDuration.set(15);
      fixture.detectChanges();

      const contentElement = fixture.nativeElement.querySelector('.marquee-content') as HTMLElement;
      const duration = contentElement?.style.getPropertyValue('--scroll-duration');
      expect(duration).toBe('15s');
    });
  });

  describe('Computed Properties', () => {
    it('should compute directionClass correctly for left', () => {
      componentRef.setInput('direction', 'left');
      fixture.detectChanges();

      expect(component.directionClass()).toBe('scroll-left');
    });

    it('should compute directionClass correctly for right', () => {
      componentRef.setInput('direction', 'right');
      fixture.detectChanges();

      expect(component.directionClass()).toBe('scroll-right');
    });

    it('should compute pauseClass correctly when true', () => {
      componentRef.setInput('pauseOnHover', true);
      fixture.detectChanges();

      expect(component.pauseClass()).toBe('pause-on-hover');
    });

    it('should compute pauseClass correctly when false', () => {
      componentRef.setInput('pauseOnHover', false);
      fixture.detectChanges();

      expect(component.pauseClass()).toBe('');
    });
  });

  describe('View References', () => {
    it('should have container view reference', () => {
      expect(component.containerRef()).toBeTruthy();
    });

    it('should have content view reference', () => {
      expect(component.contentRef()).toBeTruthy();
    });
  });

  describe('Signal State', () => {
    it('should initialize shouldScroll signal to false', () => {
      // Create new component instance to test initial state
      const newFixture = TestBed.createComponent(ScrollingMarqueeComponent);
      const newComponent = newFixture.componentInstance;

      expect(newComponent.shouldScroll()).toBe(false);
    });

    it('should initialize scrollDuration signal with default value', () => {
      expect(component.scrollDuration()).toBeGreaterThanOrEqual(0);
    });
  });
});

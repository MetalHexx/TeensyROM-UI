import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { CycleImageComponent } from './cycle-image.component';
import { ComponentRef } from '@angular/core';

describe('CycleImageComponent', () => {
  let component: CycleImageComponent;
  let fixture: ComponentFixture<CycleImageComponent>;
  let componentRef: ComponentRef<CycleImageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CycleImageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CycleImageComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
  });

  it('should create', () => {
    componentRef.setInput('images', ['image1.png']);
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display current image from array', () => {
    const images = ['image1.png', 'image2.png', 'image3.png'];
    componentRef.setInput('images', images);
    fixture.detectChanges();

    expect(component.currentImage()).toBe('image1.png');
  });

  it('should cycle through images at correct interval', fakeAsync(() => {
    const images = ['image1.png', 'image2.png', 'image3.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(0);

    // Wait for first interval
    tick(1000);
    expect(component.currentIndex()).toBe(1);

    // Wait for second interval
    tick(1000);
    expect(component.currentIndex()).toBe(2);

    // Wait for third interval - should wrap around
    tick(1000);
    expect(component.currentIndex()).toBe(0);
  }));

  it('should increment imageKey on each transition', fakeAsync(() => {
    const images = ['image1.png', 'image2.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    const initialKey = component.imageKey();

    tick(1000);
    expect(component.imageKey()).toBe(initialKey + 1);

    tick(1000);
    expect(component.imageKey()).toBe(initialKey + 2);
  }));

  it('should wrap currentIndex around to 0 after last image', fakeAsync(() => {
    const images = ['image1.png', 'image2.png'];
    componentRef.setInput('images', images);
    componentRef.setInput('intervalMs', 1000);
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(0);

    tick(1000);
    expect(component.currentIndex()).toBe(1);

    tick(1000);
    expect(component.currentIndex()).toBe(0);
  }));

  it('should handle single image gracefully', () => {
    const images = ['single-image.png'];
    componentRef.setInput('images', images);
    fixture.detectChanges();

    expect(component.currentImage()).toBe('single-image.png');
    expect(component.hasMultipleImages()).toBe(false);
  });

  it('should handle empty array gracefully', () => {
    componentRef.setInput('images', []);
    fixture.detectChanges();

    expect(component.currentImage()).toBeNull();
    expect(component.hasMultipleImages()).toBe(false);
  });

  it('should set hasMultipleImages correctly', () => {
    // Single image
    componentRef.setInput('images', ['image1.png']);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(false);

    // Multiple images
    componentRef.setInput('images', ['image1.png', 'image2.png']);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(true);

    // Empty array
    componentRef.setInput('images', []);
    fixture.detectChanges();
    expect(component.hasMultipleImages()).toBe(false);
  });
});

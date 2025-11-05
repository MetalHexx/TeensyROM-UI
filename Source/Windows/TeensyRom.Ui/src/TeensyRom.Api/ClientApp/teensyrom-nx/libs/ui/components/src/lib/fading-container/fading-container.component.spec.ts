import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FadingContainerComponent } from './fading-container.component';
import { signal } from '@angular/core';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

describe('FadingContainerComponent', () => {
  let component: FadingContainerComponent;
  let fixture: ComponentFixture<FadingContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FadingContainerComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(FadingContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render immediately by default', () => {
    expect(component['shouldRender']()).toBe(true);
  });

  it('should respect animationTrigger input', () => {
    fixture.componentRef.setInput('animationTrigger', false);
    fixture.detectChanges();

    expect(component['shouldRender']()).toBe(false);

    fixture.componentRef.setInput('animationTrigger', true);
    fixture.detectChanges();

    expect(component['shouldRender']()).toBe(true);
  });

  describe('animationParent input', () => {
    it('should wait for parent animation when animationParent is "auto"', async () => {
      const parentComplete = signal(false);

      await TestBed.resetTestingModule();
      await TestBed.configureTestingModule({
        imports: [FadingContainerComponent],
        providers: [
          provideNoopAnimations(),
          { provide: PARENT_ANIMATION_COMPLETE, useValue: parentComplete.asReadonly() },
        ],
      }).compileComponents();

      const testFixture = TestBed.createComponent(FadingContainerComponent);
      const testComponent = testFixture.componentInstance;
      testFixture.componentRef.setInput('animationParent', 'auto');
      testFixture.detectChanges();

      expect(testComponent['shouldRender']()).toBe(false);

      parentComplete.set(true);
      testFixture.detectChanges();

      expect(testComponent['shouldRender']()).toBe(true);
    });
  });

  it('should emit animationComplete event when animation finishes', () => {
    const completeSpy = vi.fn();
    component.animationComplete.subscribe(completeSpy);

    component.onAnimationDone();

    expect(completeSpy).toHaveBeenCalled();
    expect(component.animationCompleteSignal()).toBe(true);
  });

  it('should set correct animation state based on shouldRender', () => {
    fixture.componentRef.setInput('animationTrigger', true);
    fixture.detectChanges();

    expect(component['animationState']()).toBe('visible');

    fixture.componentRef.setInput('animationTrigger', false);
    fixture.detectChanges();

    expect(component['animationState']()).toBe('hidden');
  });

  it('should manage DOM presence correctly', () => {
    // Initially should be in DOM
    expect(component['shouldBeInDom']()).toBe(true);

    // Trigger hide animation
    fixture.componentRef.setInput('animationTrigger', false);
    fixture.detectChanges();

    // Should still be in DOM during animation
    expect(component['shouldBeInDom']()).toBe(true);

    // After animation completes, should be removed from DOM
    component.onAnimationDone();
    expect(component['shouldBeInDom']()).toBe(false);
  });
});

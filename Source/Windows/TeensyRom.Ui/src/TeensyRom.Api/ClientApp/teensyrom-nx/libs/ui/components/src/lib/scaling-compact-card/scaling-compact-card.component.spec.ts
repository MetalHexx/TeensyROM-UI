import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ScalingCompactCardComponent } from './scaling-compact-card.component';
import { ScalingContainerComponent } from '../scaling-container/scaling-container.component';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';

describe('ScalingCompactCardComponent', () => {
  let component: ScalingCompactCardComponent;
  let fixture: ComponentFixture<ScalingCompactCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScalingCompactCardComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(ScalingCompactCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should use default values for optional inputs', () => {
    expect(component.animationEntry()).toBe('random');
    expect(component.animationExit()).toBe('random');
    expect(component.animationTrigger()).toBe(undefined);
    expect(component.enableOverflow()).toBe(true);
  });

  it('should render by default when no animation trigger is provided', () => {
    expect(component.shouldRender()).toBe(true);
  });

  it('should use external signal when animationTrigger is provided', () => {
    const externalTrigger = signal(false);

    const newFixture = TestBed.createComponent(ScalingCompactCardComponent);
    newFixture.componentRef.setInput('animationTrigger', externalTrigger);
    newFixture.detectChanges();

    const newComponent = newFixture.componentInstance;

    expect(newComponent.shouldRender()).toBe(false);

    externalTrigger.set(true);
    newFixture.detectChanges();

    expect(newComponent.shouldRender()).toBe(true);
  });

  it('should support different animation directions', () => {
    fixture.componentRef.setInput('animationEntry', 'from-left');
    fixture.componentRef.setInput('animationExit', 'from-right');
    fixture.detectChanges();

    expect(component.animationEntry()).toBe('from-left');
    expect(component.animationExit()).toBe('from-right');
  });

  describe('animationParent input', () => {
    it('should break animation chain when animationParent is set to null', () => {
      const childFixture = TestBed.createComponent(ScalingCompactCardComponent);
      childFixture.componentRef.setInput('animationParent', null);
      childFixture.detectChanges();

      const childComponent = childFixture.componentInstance;

      // Should render immediately
      expect(childComponent.shouldRender()).toBe(true);
    });

    it('should use custom parent when animationParent is provided', () => {
      // Create custom parent
      const customParent = TestBed.createComponent(ScalingContainerComponent);
      customParent.detectChanges();
      const customParentComponent = customParent.componentInstance;

      // Create child that references custom parent
      const childFixture = TestBed.createComponent(ScalingCompactCardComponent);
      childFixture.componentRef.setInput('animationParent', customParentComponent);
      childFixture.detectChanges();

      const childComponent = childFixture.componentInstance;

      // Initially parent animation not complete
      expect(childComponent.shouldRender()).toBe(false);

      // Complete parent animation
      customParentComponent.onAnimationDone();
      childFixture.detectChanges();

      // Child should now show
      expect(childComponent.shouldRender()).toBe(true);
    });

    it('should default to normal behavior when animationParent is undefined', () => {
      const newFixture = TestBed.createComponent(ScalingCompactCardComponent);
      newFixture.detectChanges();

      const newComponent = newFixture.componentInstance;

      // Should render immediately with default behavior
      expect(newComponent.shouldRender()).toBe(true);
    });

    it('should wait for parent when animationParent is set to "auto"', () => {
      // Create component with 'auto' mode
      const childFixture = TestBed.createComponent(ScalingCompactCardComponent);
      childFixture.componentRef.setInput('animationParent', 'auto');
      childFixture.detectChanges();

      const childComponent = childFixture.componentInstance;

      // Verify the mode is set correctly
      expect(childComponent.animationParent()).toBe('auto');
      
      // Note: Full DI hierarchy testing (parent->child waiting) requires integration tests
      // This unit test verifies the input accepts 'auto' mode correctly
    });
  });
});

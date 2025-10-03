import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ScalingCardComponent } from './scaling-card.component';
import { ScalingContainerComponent } from '../scaling-container/scaling-container.component';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';

describe('ScalingCardComponent', () => {
  let component: ScalingCardComponent;
  let fixture: ComponentFixture<ScalingCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScalingCardComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(ScalingCardComponent);
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

    const newFixture = TestBed.createComponent(ScalingCardComponent);
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

  it('should pass card layout properties', () => {
    fixture.componentRef.setInput('title', 'Test Title');
    fixture.componentRef.setInput('subtitle', 'Test Subtitle');
    fixture.componentRef.setInput('metadataSource', 'Test Source');
    fixture.detectChanges();

    expect(component.title()).toBe('Test Title');
    expect(component.subtitle()).toBe('Test Subtitle');
    expect(component.metadataSource()).toBe('Test Source');
  });

  describe('animationParent input', () => {
    it('should break animation chain when animationParent is set to null', () => {
      const childFixture = TestBed.createComponent(ScalingCardComponent);
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
      const childFixture = TestBed.createComponent(ScalingCardComponent);
      childFixture.componentRef.setInput('animationParent', customParentComponent);
      childFixture.detectChanges();

      const childComponent = childFixture.componentInstance;

      // Complete parent animation
      customParentComponent.onAnimationDone();
      childFixture.detectChanges();

      // Child should now show
      expect(childComponent.shouldRender()).toBe(true);
    });

    it('should default to normal behavior when animationParent is undefined', () => {
      const newFixture = TestBed.createComponent(ScalingCardComponent);
      newFixture.detectChanges();

      const newComponent = newFixture.componentInstance;

      // Should render immediately with default behavior
      expect(newComponent.shouldRender()).toBe(true);
    });
  });
});

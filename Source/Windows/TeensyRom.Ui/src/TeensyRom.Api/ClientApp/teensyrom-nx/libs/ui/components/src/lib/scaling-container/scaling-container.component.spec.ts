import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { ScalingContainerComponent } from './scaling-container.component';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';

describe('ScalingContainerComponent', () => {
  let component: ScalingContainerComponent;
  let fixture: ComponentFixture<ScalingContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScalingContainerComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(ScalingContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should project content through ng-content', () => {
    const compiled = fixture.nativeElement;
    const containerDiv = compiled.querySelector('.scaling-container');

    expect(containerDiv).toBeTruthy();
    expect(containerDiv.classList.contains('scaling-container')).toBe(true);
  });

  it('should use default values for optional inputs', () => {
    expect(component.animationEntry()).toBe('random');
    expect(component.animationExit()).toBe('random');
    expect(component.animationTrigger()).toBe(undefined);
  });

  it('should render by default when no animation trigger is provided', () => {
    expect(component.shouldRender()).toBe(true);

    const compiled = fixture.nativeElement;
    const containerDiv = compiled.querySelector('.scaling-container');
    expect(containerDiv).toBeTruthy();
  });

  it('should use external signal when animationTrigger is provided', () => {
    const externalTrigger = signal(false);

    const newFixture = TestBed.createComponent(ScalingContainerComponent);
    newFixture.componentRef.setInput('animationTrigger', externalTrigger);
    newFixture.detectChanges();

    const newComponent = newFixture.componentInstance;

    expect(newComponent.shouldRender()).toBe(false);

    let compiled = newFixture.nativeElement;
    let containerDiv = compiled.querySelector('.scaling-container');
    expect(containerDiv).toBeFalsy();

    externalTrigger.set(true);
    newFixture.detectChanges();

    expect(newComponent.shouldRender()).toBe(true);

    compiled = newFixture.nativeElement;
    containerDiv = compiled.querySelector('.scaling-container');
    expect(containerDiv).toBeTruthy();
  });

  it('should emit animationComplete when animation finishes', () => {
    let animationCompleted = false;

    component.animationComplete.subscribe(() => {
      animationCompleted = true;
    });

    component.onAnimationDone();

    expect(animationCompleted).toBe(true);
  });

  it('should reset when external trigger changes from true to false', () => {
    const externalTrigger = signal(true);

    const newFixture = TestBed.createComponent(ScalingContainerComponent);
    newFixture.componentRef.setInput('animationTrigger', externalTrigger);
    newFixture.detectChanges();

    const newComponent = newFixture.componentInstance;
    expect(newComponent.shouldRender()).toBe(true);

    externalTrigger.set(false);
    newFixture.detectChanges();

    expect(newComponent.shouldRender()).toBe(false);
  });

  it('should support different animation directions', () => {
    fixture.componentRef.setInput('animationEntry', 'from-left');
    fixture.componentRef.setInput('animationExit', 'from-right');
    fixture.detectChanges();

    expect(component.animationEntry()).toBe('from-left');
    expect(component.animationExit()).toBe('from-right');
  });

  it('should handle none animation direction', () => {
    fixture.componentRef.setInput('animationEntry', 'none');
    fixture.detectChanges();

    const params = component.animationParams();
    expect(params.params.startTransform).toBe('translate(0, 0)');
    expect(params.params.transformOrigin).toBe('center center');
  });
});

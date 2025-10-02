import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { CardLayoutComponent } from './card-layout.component';
import { ComponentRef, Component } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

describe('CardLayoutComponent', () => {
  let component: CardLayoutComponent;
  let fixture: ComponentFixture<CardLayoutComponent>;
  let componentRef: ComponentRef<CardLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CardLayoutComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(CardLayoutComponent);
    component = fixture.componentInstance;
    componentRef = fixture.componentRef;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render mat-card with stretch-card class', () => {
    const compiled = fixture.nativeElement;
    const matCard = compiled.querySelector('mat-card');

    expect(matCard).toBeTruthy();
    expect(matCard.classList.contains('stretch-card')).toBe(true);
  });

  it('should not render header when no title is provided', () => {
    const compiled = fixture.nativeElement;
    const header = compiled.querySelector('mat-card-header');

    expect(header).toBeFalsy();
  });

  it('should render header with title when title is provided', () => {
    componentRef.setInput('title', 'Test Title');
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    const header = compiled.querySelector('mat-card-header');
    const title = compiled.querySelector('mat-card-title');

    expect(header).toBeTruthy();
    expect(title).toBeTruthy();
    expect(title.textContent?.trim()).toBe('Test Title');
  });

  it('should always render mat-card-content', () => {
    const compiled = fixture.nativeElement;
    const content = compiled.querySelector('mat-card-content');

    expect(content).toBeTruthy();
  });

  it('should project content through ng-content', () => {
    // Create a test component that uses CardLayoutComponent with content
    @Component({
      template: `
        <lib-card-layout title="Test">
          <p class="test-content">Projected content</p>
        </lib-card-layout>
      `,
      imports: [CardLayoutComponent],
    })
    class TestHostComponent {}

    const hostFixture = TestBed.createComponent(TestHostComponent);
    hostFixture.detectChanges();

    const compiled = hostFixture.nativeElement;
    const projectedContent = compiled.querySelector('.test-content');

    expect(projectedContent).toBeTruthy();
    expect(projectedContent.textContent?.trim()).toBe('Projected content');
  });

  it('should update header visibility when title changes', () => {
    const compiled = fixture.nativeElement;

    // Initially no header
    expect(compiled.querySelector('mat-card-header')).toBeFalsy();

    // Add title
    componentRef.setInput('title', 'Dynamic Title');
    fixture.detectChanges();

    let header = compiled.querySelector('mat-card-header');
    expect(header).toBeTruthy();
    expect(compiled.querySelector('mat-card-title')?.textContent?.trim()).toBe('Dynamic Title');

    // Remove title
    componentRef.setInput('title', '');
    fixture.detectChanges();

    header = compiled.querySelector('mat-card-header');
    expect(header).toBeFalsy();
  });
});

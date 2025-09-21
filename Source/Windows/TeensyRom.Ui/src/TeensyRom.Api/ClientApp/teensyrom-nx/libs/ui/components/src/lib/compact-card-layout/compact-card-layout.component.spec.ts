import { describe, it, expect, beforeEach } from 'vitest';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { CompactCardLayoutComponent } from './compact-card-layout.component';
import { Component } from '@angular/core';

describe('CompactCardLayoutComponent', () => {
  let component: CompactCardLayoutComponent;
  let fixture: ComponentFixture<CompactCardLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompactCardLayoutComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CompactCardLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render mat-card with compact-card class', () => {
    const compiled = fixture.nativeElement;
    const matCard = compiled.querySelector('mat-card');

    expect(matCard).toBeTruthy();
    expect(matCard.classList.contains('compact-card')).toBe(true);
  });

  it('should not render header or title (compact design)', () => {
    const compiled = fixture.nativeElement;
    const header = compiled.querySelector('mat-card-header');
    const title = compiled.querySelector('mat-card-title');

    expect(header).toBeFalsy();
    expect(title).toBeFalsy();
  });

  it('should project content through ng-content', () => {
    // Create a test component that uses CompactCardLayoutComponent with content
    @Component({
      template: `
        <lib-compact-card-layout>
          <p class="test-content">Projected content</p>
        </lib-compact-card-layout>
      `,
      imports: [CompactCardLayoutComponent],
    })
    class TestHostComponent {}

    const hostFixture = TestBed.createComponent(TestHostComponent);
    hostFixture.detectChanges();

    const compiled = hostFixture.nativeElement;
    const projectedContent = compiled.querySelector('.test-content');

    expect(projectedContent).toBeTruthy();
    expect(projectedContent.textContent?.trim()).toBe('Projected content');
  });

  it('should work with form fields (main use case)', () => {
    // Create a test component with form field like search toolbar
    @Component({
      template: `
        <lib-compact-card-layout>
          <div class="form-content">
            <input type="text" placeholder="Search..." />
          </div>
        </lib-compact-card-layout>
      `,
      imports: [CompactCardLayoutComponent],
    })
    class TestFormComponent {}

    const hostFixture = TestBed.createComponent(TestFormComponent);
    hostFixture.detectChanges();

    const compiled = hostFixture.nativeElement;
    const formContent = compiled.querySelector('.form-content');
    const input = compiled.querySelector('input');

    expect(formContent).toBeTruthy();
    expect(input).toBeTruthy();
    expect(input.placeholder).toBe('Search...');
  });

  it('should maintain compact card styling for multiple children', () => {
    @Component({
      template: `
        <lib-compact-card-layout>
          <div class="child1">Child 1</div>
          <div class="child2">Child 2</div>
          <button class="child3">Button</button>
        </lib-compact-card-layout>
      `,
      imports: [CompactCardLayoutComponent],
    })
    class TestMultipleChildrenComponent {}

    const hostFixture = TestBed.createComponent(TestMultipleChildrenComponent);
    hostFixture.detectChanges();

    const compiled = hostFixture.nativeElement;
    const matCard = compiled.querySelector('mat-card');
    const children = compiled.querySelectorAll('div, button');

    expect(matCard.classList.contains('compact-card')).toBe(true);
    expect(children.length).toBe(3);
    expect(compiled.querySelector('.child1')).toBeTruthy();
    expect(compiled.querySelector('.child2')).toBeTruthy();
    expect(compiled.querySelector('.child3')).toBeTruthy();
  });
});

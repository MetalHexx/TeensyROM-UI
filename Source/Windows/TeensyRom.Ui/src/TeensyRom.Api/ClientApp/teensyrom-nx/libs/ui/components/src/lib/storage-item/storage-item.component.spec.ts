import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { StorageItemComponent } from './storage-item.component';
import { StorageItemActionsComponent } from './storage-item-actions.component';
import { Component } from '@angular/core';

describe('StorageItemComponent', () => {
  let component: StorageItemComponent;
  let fixture: ComponentFixture<StorageItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideNoopAnimations()],
      imports: [StorageItemComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(StorageItemComponent);
    component = fixture.componentInstance;
  });

  describe('Component Creation', () => {
    it('should create', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test Item');
      fixture.detectChanges();
      expect(component).toBeTruthy();
    });
  });

  describe('Inputs', () => {
    it('should accept icon input', () => {
      fixture.componentRef.setInput('icon', 'music_note');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(component.icon()).toBe('music_note');
    });

    it('should accept label input', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'My File');
      fixture.detectChanges();
      expect(component.label()).toBe('My File');
    });

    it('should accept optional iconColor input', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('iconColor', 'directory');
      fixture.detectChanges();
      expect(component.iconColor()).toBe('directory');
    });

    it('should default iconColor to "normal"', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(component.iconColor()).toBe('normal');
    });


    it('should accept selected input', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('selected', true);
      fixture.detectChanges();
      expect(component.selected()).toBe(true);
    });

    it('should default selected to false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(component.selected()).toBe(false);
    });

    it('should accept active input', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('active', true);
      fixture.detectChanges();
      expect(component.active()).toBe(true);
    });

    it('should default active to false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(component.active()).toBe(false);
    });

    it('should accept disabled input', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      expect(component.disabled()).toBe(true);
    });

    it('should default disabled to false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(component.disabled()).toBe(false);
    });
  });

  describe('CSS Classes', () => {
    it('should apply selected class when selected is true', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('selected', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('selected')).toBe(true);
    });

    it('should not apply selected class when selected is false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('selected', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('selected')).toBe(false);
    });

    it('should apply active class when active is true', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('active', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('active')).toBe(true);
    });

    it('should not apply active class when active is false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('active', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('active')).toBe(false);
    });

    it('should apply disabled class when disabled is true', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('disabled')).toBe(true);
    });

    it('should not apply disabled class when disabled is false', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.classList.contains('disabled')).toBe(false);
    });
  });

  describe('Accessibility', () => {
    it('should have role="button"', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('role')).toBe('button');
    });

    it('should have tabindex="0" when not disabled', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('tabindex')).toBe('0');
    });

    it('should have tabindex="-1" when disabled', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('tabindex')).toBe('-1');
    });

    it('should have aria-selected="true" when selected', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('selected', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('aria-selected')).toBe('true');
    });

    it('should have aria-selected="false" when not selected', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('selected', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('aria-selected')).toBe('false');
    });

    it('should have aria-disabled="true" when disabled', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('aria-disabled')).toBe('true');
    });

    it('should not have aria-disabled when not disabled', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
      fixture.componentRef.setInput('disabled', false);
      fixture.detectChanges();
      expect(fixture.nativeElement.getAttribute('aria-disabled')).toBeNull();
    });
  });

  describe('Event Handling', () => {
    beforeEach(() => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'Test');
    });

    it('should emit selectedChange on click', () => {
      fixture.detectChanges();
      let emitted = false;
      component.selectedChange.subscribe(() => {
        emitted = true;
      });

      fixture.nativeElement.click();
      expect(emitted).toBe(true);
    });

    it('should not emit selectedChange on click when disabled', () => {
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      let emitted = false;
      component.selectedChange.subscribe(() => {
        emitted = true;
      });

      fixture.nativeElement.click();
      expect(emitted).toBe(false);
    });

    it('should emit activated on double-click', () => {
      fixture.detectChanges();
      let emitted = false;
      component.activated.subscribe(() => {
        emitted = true;
      });

      fixture.nativeElement.dispatchEvent(new MouseEvent('dblclick'));
      expect(emitted).toBe(true);
    });

    it('should not emit activated on double-click when disabled', () => {
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      let emitted = false;
      component.activated.subscribe(() => {
        emitted = true;
      });

      fixture.nativeElement.dispatchEvent(new MouseEvent('dblclick'));
      expect(emitted).toBe(false);
    });

    it('should emit activated on Enter key press', () => {
      fixture.detectChanges();
      let emitted = false;
      component.activated.subscribe(() => {
        emitted = true;
      });

      const event = new KeyboardEvent('keydown', { key: 'Enter' });
      fixture.nativeElement.dispatchEvent(event);
      expect(emitted).toBe(true);
    });

    it('should not emit activated on Enter key when disabled', () => {
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      let emitted = false;
      component.activated.subscribe(() => {
        emitted = true;
      });

      const event = new KeyboardEvent('keydown', { key: 'Enter' });
      fixture.nativeElement.dispatchEvent(event);
      expect(emitted).toBe(false);
    });

    it('should emit selectedChange on Space key press', () => {
      fixture.detectChanges();
      let emitted = false;
      component.selectedChange.subscribe(() => {
        emitted = true;
      });

      const event = new KeyboardEvent('keydown', { key: ' ' });
      fixture.nativeElement.dispatchEvent(event);
      expect(emitted).toBe(true);
    });

    it('should not emit selectedChange on Space key when disabled', () => {
      fixture.componentRef.setInput('disabled', true);
      fixture.detectChanges();
      let emitted = false;
      component.selectedChange.subscribe(() => {
        emitted = true;
      });

      const event = new KeyboardEvent('keydown', { key: ' ' });
      fixture.nativeElement.dispatchEvent(event);
      expect(emitted).toBe(false);
    });
  });

  describe('Template Rendering', () => {
    it('should render label text', () => {
      fixture.componentRef.setInput('icon', 'folder');
      fixture.componentRef.setInput('label', 'My File Name');
      fixture.detectChanges();
      expect(fixture.nativeElement.textContent).toContain('My File Name');
    });
  });

  describe('Content Projection', () => {
    @Component({
      template: `
        <lib-storage-item icon="music_note" label="Test Song">
          <lib-storage-item-actions label="1.5 KB">
            <button class="test-button">Play</button>
          </lib-storage-item-actions>
        </lib-storage-item>
      `,
      imports: [StorageItemComponent, StorageItemActionsComponent],
    })
    class TestHostComponent {}

    it('should project lib-storage-item-actions content', async () => {
      const hostFixture = TestBed.createComponent(TestHostComponent);
      hostFixture.detectChanges();

      const button = hostFixture.nativeElement.querySelector('.test-button');
      expect(button).toBeTruthy();
      expect(button.textContent).toContain('Play');
    });

    it('should render lib-storage-item-actions label', async () => {
      const hostFixture = TestBed.createComponent(TestHostComponent);
      hostFixture.detectChanges();

      expect(hostFixture.nativeElement.textContent).toContain('1.5 KB');
    });
  });
});

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EmptyStateMessageComponent } from './empty-state-message.component';

describe('EmptyStateMessageComponent', () => {
  let component: EmptyStateMessageComponent;
  let fixture: ComponentFixture<EmptyStateMessageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EmptyStateMessageComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(EmptyStateMessageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display required icon and title', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const icon = compiled.querySelector('.empty-state-icon');
    const title = compiled.querySelector('.empty-state-title');

    expect(icon?.textContent?.trim()).toBe('devices');
    expect(title?.textContent?.trim()).toBe('No Devices');
  });

  it('should display optional message when provided', () => {
    fixture.componentRef.setInput('icon', 'search');
    fixture.componentRef.setInput('title', 'No Results');
    fixture.componentRef.setInput('message', 'Try different keywords');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const message = compiled.querySelector('.empty-state-message-text');

    expect(message?.textContent?.trim()).toBe('Try different keywords');
  });

  it('should not display message when not provided', () => {
    fixture.componentRef.setInput('icon', 'search');
    fixture.componentRef.setInput('title', 'No Results');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const message = compiled.querySelector('.empty-state-message-text');

    expect(message).toBeNull();
  });

  it('should display optional secondary message when provided', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.componentRef.setInput('secondaryMessage', 'Visit Device View');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const secondaryMessage = compiled.querySelector('.empty-state-secondary-text');

    expect(secondaryMessage?.textContent?.trim()).toBe('Visit Device View');
  });

  it('should not display secondary message when not provided', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const secondaryMessage = compiled.querySelector('.empty-state-secondary-text');

    expect(secondaryMessage).toBeNull();
  });

  it('should apply medium size class by default', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('.empty-state-message');

    expect(container?.classList.contains('size-medium')).toBe(true);
  });

  it('should apply small size class when specified', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.componentRef.setInput('size', 'small');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('.empty-state-message');

    expect(container?.classList.contains('size-small')).toBe(true);
    expect(container?.classList.contains('size-medium')).toBe(false);
  });

  it('should apply large size class when specified', () => {
    fixture.componentRef.setInput('icon', 'devices');
    fixture.componentRef.setInput('title', 'No Devices');
    fixture.componentRef.setInput('size', 'large');
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const container = compiled.querySelector('.empty-state-message');

    expect(container?.classList.contains('size-large')).toBe(true);
    expect(container?.classList.contains('size-medium')).toBe(false);
  });
});

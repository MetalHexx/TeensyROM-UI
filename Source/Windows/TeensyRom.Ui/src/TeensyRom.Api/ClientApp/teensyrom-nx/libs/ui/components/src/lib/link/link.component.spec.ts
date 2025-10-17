import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LinkComponent } from './link.component';
import { IconLabelComponent } from '../icon-label/icon-label.component';

describe('LinkComponent', () => {
  let component: LinkComponent;
  let fixture: ComponentFixture<LinkComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LinkComponent, IconLabelComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(LinkComponent);
    component = fixture.componentInstance;
    // Set required input before detectChanges
    fixture.componentRef.setInput('label', 'Test Label');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render label text', () => {
    const iconLabel = fixture.debugElement.nativeElement.querySelector('lib-icon-label');
    expect(iconLabel).toBeTruthy();
  });

  it('should display the provided Material icon', () => {
    fixture.componentRef.setInput('icon', 'edit');
    fixture.detectChanges();
    const iconLabel = fixture.debugElement.nativeElement.querySelector('lib-icon-label');
    expect(iconLabel).toBeTruthy();
    expect(iconLabel.getAttribute('ng-reflect-icon')).toContain('edit');
  });

  it('should use default icon when not specified', () => {
    expect(component.icon()).toBe('link');
  });

  it('should apply the specified icon color variant', () => {
    fixture.componentRef.setInput('iconColor', 'success');
    fixture.detectChanges();
    const iconLabel = fixture.debugElement.nativeElement.querySelector('lib-icon-label');
    expect(iconLabel).toBeTruthy();
    expect(iconLabel.getAttribute('ng-reflect-color')).toContain('success');
  });

  it('should use default color when not specified', () => {
    expect(component.iconColor()).toBe('primary');
  });
});

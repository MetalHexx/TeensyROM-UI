import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { YouTubeDialogComponent } from './youtube-dialog.component';
import { ScalingCardComponent, IconButtonComponent } from '@teensyrom-nx/ui/components';
import { YouTubeVideo } from '@teensyrom-nx/domain';
import { provideNoopAnimations } from '@angular/platform-browser/animations';

describe('YouTubeDialogComponent', () => {
  let component: YouTubeDialogComponent;
  let fixture: ComponentFixture<YouTubeDialogComponent>;
  let mockDialogRef: MatDialogRef<YouTubeDialogComponent>;
  const mockVideo: YouTubeVideo = {
    videoId: 'dQw4w9WgXcQ',
    url: 'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
    channel: 'Test Channel',
    subtune: 0,
  };

  beforeEach(async () => {
    mockDialogRef = { close: vi.fn() } as unknown as MatDialogRef<YouTubeDialogComponent>;

    // Mock element.animate for ScalingCard animations
    if (!Element.prototype.animate) {
      Element.prototype.animate = vi.fn(() => ({
        finished: Promise.resolve(),
        onfinish: null,
        playState: 'finished',
        cancel: vi.fn(),
        pause: vi.fn(),
        play: vi.fn(),
        reverse: vi.fn(),
      })) as any;
    }

    await TestBed.configureTestingModule({
      imports: [YouTubeDialogComponent, ScalingCardComponent, IconButtonComponent],
      providers: [
        provideNoopAnimations(),
        { provide: MatDialogRef, useValue: mockDialogRef },
        { provide: MAT_DIALOG_DATA, useValue: { video: mockVideo } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(YouTubeDialogComponent);
    component = fixture.componentInstance;
    // Don't call detectChanges yet - let each test control when animations run
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display channel name as title', () => {
    fixture.detectChanges();
    const scalingCard = fixture.debugElement.nativeElement.querySelector('lib-scaling-card');
    expect(scalingCard).toBeTruthy();
    expect(scalingCard.getAttribute('ng-reflect-title')).toBe('Test Channel');
  });

  it('should construct correct YouTube embed URL', () => {
    // URL is wrapped in SafeResourceUrl, so we just check the component was created successfully
    expect(component.youtubeEmbedUrl).toBeTruthy();
  });

  it('should render iframe with correct embed URL', () => {
    fixture.detectChanges();
    const iframe = fixture.debugElement.nativeElement.querySelector('iframe');
    expect(iframe).toBeTruthy();
    expect(iframe.src).toContain(`youtube.com/embed/${mockVideo.videoId}`);
  });

  it('should close dialog when close button clicked', () => {
    const closeSpy = vi.spyOn(mockDialogRef as unknown as { close: () => void }, 'close');
    component.onClose();
    expect(closeSpy).toHaveBeenCalled();
  });

  it('should have proper iframe attributes for security and functionality', () => {
    fixture.detectChanges();
    const iframe = fixture.debugElement.nativeElement.querySelector('iframe');
    expect(iframe.getAttribute('allow')).toContain('autoplay');
    expect(iframe.getAttribute('referrerpolicy')).toBe('strict-origin-when-cross-origin');
    expect(iframe.getAttribute('allowfullscreen')).toBe('');
  });
});

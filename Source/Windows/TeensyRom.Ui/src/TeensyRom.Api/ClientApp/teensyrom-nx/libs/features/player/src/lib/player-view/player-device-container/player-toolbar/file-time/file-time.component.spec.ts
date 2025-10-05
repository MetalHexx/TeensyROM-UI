import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileTimeComponent } from './file-time.component';

describe('FileTimeComponent', () => {
  let component: FileTimeComponent;
  let fixture: ComponentFixture<FileTimeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileTimeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(FileTimeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should format time correctly without hours', () => {
    fixture.componentRef.setInput('currentTime', 4000);  // 4 seconds in milliseconds
    fixture.componentRef.setInput('totalTime', 514000);  // 514 seconds in milliseconds
    fixture.componentRef.setInput('show', true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-time')?.textContent).toBe('00:04 / 08:34');
  });

  it('should handle zero time', () => {
    fixture.componentRef.setInput('currentTime', 0);
    fixture.componentRef.setInput('totalTime', 0);
    fixture.componentRef.setInput('show', true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-time')?.textContent).toBe('00:00 / 00:00');
  });

  it('should format time with hours when >= 1 hour', () => {
    fixture.componentRef.setInput('currentTime', 3661000);  // 3661 seconds in milliseconds
    fixture.componentRef.setInput('totalTime', 7200000);    // 7200 seconds in milliseconds
    fixture.componentRef.setInput('show', true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-time')?.textContent).toBe('1:01:01 / 2:00:00');
  });

  it('should not display when show is false', () => {
    fixture.componentRef.setInput('currentTime', 4000);
    fixture.componentRef.setInput('totalTime', 514000);
    fixture.componentRef.setInput('show', false);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-time')).toBeNull();
  });

  it('should handle negative time values', () => {
    fixture.componentRef.setInput('currentTime', -10);
    fixture.componentRef.setInput('totalTime', 100000);  // 100 seconds in milliseconds
    fixture.componentRef.setInput('show', true);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-time')?.textContent).toBe('00:00 / 01:40');
  });
});

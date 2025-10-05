import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileInfoComponent } from './file-info.component';
import { FileItem, FileItemType } from '@teensyrom-nx/domain';

describe('FileInfoComponent', () => {
  let component: FileInfoComponent;
  let fixture: ComponentFixture<FileInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileInfoComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(FileInfoComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display file title, creator, and release info', () => {
    const mockFile: FileItem = {
      name: 'test.sid',
      path: '/music/test.sid',
      size: 1024,
      isFavorite: false,
      isCompatible: true,
      title: 'Test Song',
      creator: 'Test Artist',
      releaseInfo: '2024',
      description: '',
      shareUrl: '',
      metadataSource: 'HVSC',
      meta1: '',
      meta2: '',
      metadataSourcePath: '',
      parentPath: '/music',
      playLength: '3:30',
      subtuneLengths: [],
      startSubtuneNum: 1,
      images: [],
      type: FileItemType.Song
    };

    fixture.componentRef.setInput('fileItem', mockFile);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-title')?.textContent).toBe('Test Song');
    expect(compiled.querySelector('.file-creator')?.textContent).toBe('Test Artist');
    expect(compiled.querySelector('.file-metadata')?.textContent).toBe('2024');
  });

  it('should handle null fileItem gracefully', () => {
    fixture.componentRef.setInput('fileItem', null);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-info')).toBeNull();
  });

  it('should fallback to metadataSource when releaseInfo is empty', () => {
    const mockFile: FileItem = {
      name: 'test.sid',
      path: '/music/test.sid',
      size: 1024,
      isFavorite: false,
      isCompatible: true,
      title: 'Test Song',
      creator: 'Test Artist',
      releaseInfo: '',
      description: '',
      shareUrl: '',
      metadataSource: 'HVSC',
      meta1: '',
      meta2: '',
      metadataSourcePath: '',
      parentPath: '/music',
      playLength: '3:30',
      subtuneLengths: [],
      startSubtuneNum: 1,
      images: [],
      type: FileItemType.Song
    };

    fixture.componentRef.setInput('fileItem', mockFile);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.file-metadata')?.textContent).toBe('HVSC');
  });
});

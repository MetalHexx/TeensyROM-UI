import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileItemComponent } from './file-item.component';
import { FileItem, FileItemType } from '@teensyrom-nx/domain';

describe('FileItemComponent', () => {
  let component: FileItemComponent;
  let fixture: ComponentFixture<FileItemComponent>;

  const createMockFileItem = (type: FileItemType, size: number): FileItem => ({
    name: 'test-file',
    path: '/test/path',
    size,
    type,
    isFavorite: false,
    title: '',
    creator: '',
    releaseInfo: '',
    description: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    parentPath: '',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
    images: [],
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileItemComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FileItemComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 0));
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should map Unknown to insert_drive_file icon', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 0));
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('insert_drive_file');
  });

  it('should map Song to music_note icon', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Song, 0));
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('music_note');
  });

  it('should map Game to sports_esports icon', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Game, 0));
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('sports_esports');
  });

  it('should map Image to image icon', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Image, 0));
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('image');
  });

  it('should map Hex to code icon', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Hex, 0));
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('code');
  });

  it('should format 0 bytes correctly', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 0));
    fixture.detectChanges();
    expect(component.formattedSize()).toBe('0 B');
  });

  it('should format bytes correctly', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 512));
    fixture.detectChanges();
    expect(component.formattedSize()).toBe('512.0 B');
  });

  it('should format kilobytes correctly', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 1536));
    fixture.detectChanges();
    expect(component.formattedSize()).toBe('1.5 KB');
  });

  it('should format megabytes correctly', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 2359296));
    fixture.detectChanges();
    expect(component.formattedSize()).toBe('2.3 MB');
  });

  it('should format gigabytes correctly', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 3221225472));
    fixture.detectChanges();
    expect(component.formattedSize()).toBe('3.0 GB');
  });

  it('should emit itemSelected on click', () => {
    const mockFile = createMockFileItem(FileItemType.Song, 1024);
    fixture.componentRef.setInput('fileItem', mockFile);
    fixture.detectChanges();

    let emittedItem: FileItem | undefined;
    component.itemSelected.subscribe((item) => {
      emittedItem = item;
    });

    const fileElement = fixture.nativeElement.querySelector('.file-item');
    fileElement.click();

    expect(emittedItem).toEqual(mockFile);
  });

  it('should apply selected class when selected is true', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 0));
    fixture.componentRef.setInput('selected', true);
    fixture.detectChanges();

    const fileElement = fixture.nativeElement.querySelector('.file-item');
    expect(fileElement.classList.contains('selected')).toBe(true);
  });

  it('should not apply selected class when selected is false', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 0));
    fixture.componentRef.setInput('selected', false);
    fixture.detectChanges();

    const fileElement = fixture.nativeElement.querySelector('.file-item');
    expect(fileElement.classList.contains('selected')).toBe(false);
  });

  it('should render file name', () => {
    const mockFile = createMockFileItem(FileItemType.Song, 1024);
    fixture.componentRef.setInput('fileItem', mockFile);
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('test-file');
  });

  it('should render formatted file size', () => {
    fixture.componentRef.setInput('fileItem', createMockFileItem(FileItemType.Unknown, 1536));
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('1.5 KB');
  });
});

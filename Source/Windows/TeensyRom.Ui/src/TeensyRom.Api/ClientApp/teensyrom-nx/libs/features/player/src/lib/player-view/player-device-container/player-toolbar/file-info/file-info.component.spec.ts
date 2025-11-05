import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
import { FileInfoComponent } from './file-info.component';
import { FileItem, FileItemType } from '@teensyrom-nx/domain';
import { describe, it, expect, beforeEach } from 'vitest';

describe('FileInfoComponent', () => {
  let component: FileInfoComponent;
  let fixture: ComponentFixture<FileInfoComponent>;

  const createMockFile = (overrides: Partial<FileItem> = {}): FileItem => ({
    name: 'test.file',
    path: '/test/test.file',
    size: 1024,
    isFavorite: false,
    isCompatible: true,
    title: 'Test File',
    creator: 'Test Creator',
    releaseInfo: '2024',
    description: '',
    shareUrl: '',
    metadataSource: '',
    meta1: '',
    meta2: '',
    metadataSourcePath: '',
    parentPath: '/test',
    playLength: '',
    subtuneLengths: [],
    startSubtuneNum: 0,
    images: [],
    type: FileItemType.Game,
    ...overrides,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FileInfoComponent],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    fixture = TestBed.createComponent(FileInfoComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should accept null fileItem input', () => {
      fixture.componentRef.setInput('fileItem', null);
      fixture.detectChanges();

      expect(component.fileItem()).toBeNull();
    });
  });

  describe('File Display', () => {
    it('should display file title and creator', () => {
      const mockFile = createMockFile({
        title: 'Test Song',
        creator: 'Test Artist',
        type: FileItemType.Song,
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-title')?.textContent).toBe('Test Song');
      expect(compiled.querySelector('.file-creator')?.textContent).toBe('Test Artist');
    });

    it('should handle null fileItem gracefully', () => {
      fixture.componentRef.setInput('fileItem', null);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-info')).toBeNull();
    });

    it('should render file-info container when fileItem exists', () => {
      const mockFile = createMockFile();
      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-info')).toBeTruthy();
      expect(compiled.querySelector('.file-text')).toBeTruthy();
    });

    it('should display empty title when title is not provided', () => {
      const mockFile = createMockFile({ title: '' });
      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      const titleElement = compiled.querySelector('.file-title');
      expect(titleElement?.textContent).toBe('');
    });
  });

  describe('Creator Display Logic', () => {
    it('should show creator when creator is provided', () => {
      const mockFile = createMockFile({
        creator: 'Rob Hubbard',
        meta1: 'sid',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-creator')?.textContent).toBe('Rob Hubbard');
    });

    it('should show file type name when creator is empty', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: 'prg',
        title: 'Test Game',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-creator')?.textContent).toBe('Program');
    });

    it('should not show creator div when both creator and fileTypeName are empty', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: '',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-creator')).toBeNull();
    });
  });

  describe('File Type Name Mapping', () => {
    const typeTestCases: Array<{ meta1: string; expected: string; description: string }> = [
      { meta1: 'sid', expected: 'Music', description: 'SID music files' },
      { meta1: 'crt', expected: 'Cartridge', description: 'Cartridge files' },
      { meta1: 'prg', expected: 'Program', description: 'Program files' },
      { meta1: 'p00', expected: 'Program', description: 'P00 program files' },
      { meta1: 'hex', expected: 'Machine Code', description: 'HEX files' },
      { meta1: 'kla', expected: 'Koala Image', description: 'Koala KLA files' },
      { meta1: 'koa', expected: 'Koala Image', description: 'Koala KOA files' },
      { meta1: 'art', expected: 'Art Studio Image', description: 'Art Studio files' },
      { meta1: 'aas', expected: 'Art Studio Image', description: 'Advanced Art Studio files' },
      { meta1: 'hpi', expected: 'HiRes Image', description: 'HiRes image files' },
      { meta1: 'd64', expected: 'Disk Image', description: 'D64 disk images' },
      { meta1: 'seq', expected: 'Sequential File', description: 'Sequential files' },
      { meta1: 'txt', expected: 'Text File', description: 'Text files' },
      { meta1: 'zip', expected: 'Archive', description: 'ZIP archives' },
      { meta1: 'nfo', expected: 'Info File', description: 'NFO info files' },
      { meta1: 'unknown', expected: 'Unknown', description: 'Unknown file types' },
    ];

    typeTestCases.forEach(({ meta1, expected, description }) => {
      it(`should map ${meta1} to "${expected}" for ${description}`, () => {
        const mockFile = createMockFile({
          creator: '',
          meta1,
        });

        fixture.componentRef.setInput('fileItem', mockFile);
        fixture.detectChanges();

        expect(component.fileTypeName()).toBe(expected);
      });
    });

    it('should handle uppercase meta1 by converting to lowercase', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: 'SID',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.fileTypeName()).toBe('Music');
    });

    it('should handle mixed case meta1', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: 'PrG',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.fileTypeName()).toBe('Program');
    });

    it('should return uppercase meta1 for unmapped file types', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: 'xyz',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.fileTypeName()).toBe('XYZ');
    });

    it('should return empty string when meta1 is empty', () => {
      const mockFile = createMockFile({
        creator: '',
        meta1: '',
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.fileTypeName()).toBe('');
    });

    it('should return empty string when meta1 is undefined', () => {
      const mockFile = createMockFile({
        creator: '',
      });
      // Explicitly set meta1 to undefined
      (mockFile as { meta1?: string }).meta1 = undefined;

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.fileTypeName()).toBe('');
    });
  });

  describe('Image Handling', () => {
    it('should return empty array when fileItem is null', () => {
      fixture.componentRef.setInput('fileItem', null);
      fixture.detectChanges();

      expect(component.imageUrls()).toEqual([]);
    });

    it('should return empty array when images array is empty', () => {
      const mockFile = createMockFile({ images: [] });
      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.imageUrls()).toEqual([]);
    });

    it('should return empty array when images is undefined', () => {
      const mockFile = createMockFile();
      // Remove the images property
      (mockFile as { images?: unknown }).images = undefined;
      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.imageUrls()).toEqual([]);
    });

    it('should extract image URLs from single image', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/image1.png',
            fileName: 'image1.png',
            path: '/images/image1.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.imageUrls()).toEqual(['https://example.com/image1.png']);
    });

    it('should extract image URLs from multiple images', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/image1.png',
            fileName: 'image1.png',
            path: '/images/image1.png',
            source: 'gamebase',
          },
          {
            url: 'https://example.com/image2.jpg',
            fileName: 'image2.jpg',
            path: '/images/image2.jpg',
            source: 'gamebase',
          },
          {
            url: 'https://example.com/image3.gif',
            fileName: 'image3.gif',
            path: '/images/image3.gif',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      expect(component.imageUrls()).toEqual([
        'https://example.com/image1.png',
        'https://example.com/image2.jpg',
        'https://example.com/image3.gif',
      ]);
    });

    it('should pass imageUrls to CycleImageComponent', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/img1.png',
            fileName: 'img1.png',
            path: '/images/img1.png',
            source: 'gamebase',
          },
          {
            url: 'https://example.com/img2.png',
            fileName: 'img2.png',
            path: '/images/img2.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const cycleImage = fixture.debugElement.query(By.css('lib-cycle-image'));
      expect(cycleImage).toBeTruthy();
      expect(cycleImage.componentInstance.images()).toEqual([
        'https://example.com/img1.png',
        'https://example.com/img2.png',
      ]);
    });

    it('should pass intervalMs of 4000 to CycleImageComponent', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/img.png',
            fileName: 'img.png',
            path: '/images/img.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const cycleImage = fixture.debugElement.query(By.css('lib-cycle-image'));
      expect(cycleImage.componentInstance.intervalMs()).toBe(4000);
    });

    it('should pass thumbnail size to CycleImageComponent', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/img.png',
            fileName: 'img.png',
            path: '/images/img.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const cycleImage = fixture.debugElement.query(By.css('lib-cycle-image'));
      expect(cycleImage.componentInstance.size()).toBe('thumbnail');
    });
  });

  describe('Computed Signal Reactivity', () => {
    it('should update imageUrls when fileItem changes', () => {
      const mockFile1 = createMockFile({
        images: [
          {
            url: 'https://example.com/image1.png',
            fileName: 'image1.png',
            path: '/images/image1.png',
            source: 'gamebase',
          },
        ],
      });
      const mockFile2 = createMockFile({
        images: [
          {
            url: 'https://example.com/image2.png',
            fileName: 'image2.png',
            path: '/images/image2.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile1);
      fixture.detectChanges();
      expect(component.imageUrls()).toEqual(['https://example.com/image1.png']);

      fixture.componentRef.setInput('fileItem', mockFile2);
      fixture.detectChanges();
      expect(component.imageUrls()).toEqual(['https://example.com/image2.png']);
    });

    it('should update fileTypeName when fileItem changes', () => {
      const mockFile1 = createMockFile({ creator: '', meta1: 'sid' });
      const mockFile2 = createMockFile({ creator: '', meta1: 'prg' });

      fixture.componentRef.setInput('fileItem', mockFile1);
      fixture.detectChanges();
      expect(component.fileTypeName()).toBe('Music');

      fixture.componentRef.setInput('fileItem', mockFile2);
      fixture.detectChanges();
      expect(component.fileTypeName()).toBe('Program');
    });

    it('should update DOM when fileItem changes', () => {
      const mockFile1 = createMockFile({
        title: 'First File',
        creator: 'Creator 1',
      });
      const mockFile2 = createMockFile({
        title: 'Second File',
        creator: 'Creator 2',
      });

      fixture.componentRef.setInput('fileItem', mockFile1);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-title')?.textContent).toBe('First File');
      expect(compiled.querySelector('.file-creator')?.textContent).toBe('Creator 1');

      fixture.componentRef.setInput('fileItem', mockFile2);
      fixture.detectChanges();

      expect(compiled.querySelector('.file-title')?.textContent).toBe('Second File');
      expect(compiled.querySelector('.file-creator')?.textContent).toBe('Creator 2');
    });
  });

  describe('Component Integration', () => {
    it('should render CycleImageComponent when file has images', () => {
      const mockFile = createMockFile({
        images: [
          {
            url: 'https://example.com/img.png',
            fileName: 'img.png',
            path: '/images/img.png',
            source: 'gamebase',
          },
        ],
      });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const cycleImage = fixture.debugElement.query(By.css('lib-cycle-image'));
      expect(cycleImage).toBeTruthy();
    });

    it('should render CycleImageComponent with empty images array when no images', () => {
      const mockFile = createMockFile({ images: [] });

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const cycleImage = fixture.debugElement.query(By.css('lib-cycle-image'));
      expect(cycleImage).toBeTruthy();
      expect(cycleImage.componentInstance.images()).toEqual([]);
    });

    it('should have file-info and file-text CSS classes for styling', () => {
      const mockFile = createMockFile();

      fixture.componentRef.setInput('fileItem', mockFile);
      fixture.detectChanges();

      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('.file-info')).toBeTruthy();
      expect(compiled.querySelector('.file-text')).toBeTruthy();
      expect(compiled.querySelector('.file-title')).toBeTruthy();
    });
  });
});

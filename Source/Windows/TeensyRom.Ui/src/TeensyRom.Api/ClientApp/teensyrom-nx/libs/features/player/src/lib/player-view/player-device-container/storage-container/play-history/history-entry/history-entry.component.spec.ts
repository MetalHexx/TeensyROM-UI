import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { HistoryEntryComponent } from './history-entry.component';
import { HistoryEntry } from '@teensyrom-nx/application';
import { FileItem, FileItemType, LaunchMode, StorageType } from '@teensyrom-nx/domain';
import { StorageKeyUtil } from '@teensyrom-nx/application';

describe('HistoryEntryComponent', () => {
  let component: HistoryEntryComponent;
  let fixture: ComponentFixture<HistoryEntryComponent>;

  const createMockFileItem = (type: FileItemType, size: number, name: string): FileItem => ({
    name,
    path: '/test/path',
    size,
    type,
    isFavorite: false,
    isCompatible: true,
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

  const createMockHistoryEntry = (
    type: FileItemType,
    size: number,
    name: string,
    timestamp: number
  ): HistoryEntry => ({
    file: createMockFileItem(type, size, name),
    storageKey: StorageKeyUtil.create('test-device', StorageType.Usb),
    parentPath: '/test',
    launchMode: LaunchMode.Shuffle,
    timestamp,
    isCompatible: true,
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideNoopAnimations()],
      imports: [HistoryEntryComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(HistoryEntryComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    const entry = createMockHistoryEntry(FileItemType.Song, 1024, 'test-song.sid', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('should display file name', () => {
    const entry = createMockHistoryEntry(
      FileItemType.Song,
      1024,
      'my-favorite-song.sid',
      Date.now()
    );
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('my-favorite-song.sid');
  });

  it('should display formatted timestamp', () => {
    // Create a specific timestamp for testing: 3:45 PM
    const testDate = new Date(2025, 9, 9, 15, 45, 0); // October 9, 2025, 3:45 PM
    const entry = createMockHistoryEntry(
      FileItemType.Song,
      1024,
      'test-song.sid',
      testDate.getTime()
    );
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    // The exact format depends on locale, but should contain hour and minute
    expect(compiled.textContent).toMatch(/3:45\s*PM/);
  });

  it('should map Song to music_note icon', () => {
    const entry = createMockHistoryEntry(FileItemType.Song, 1024, 'test.sid', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('music_note');
  });

  it('should map Game to sports_esports icon', () => {
    const entry = createMockHistoryEntry(FileItemType.Game, 2048, 'test.prg', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('sports_esports');
  });

  it('should map Image to image icon', () => {
    const entry = createMockHistoryEntry(FileItemType.Image, 512, 'test.png', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('image');
  });

  it('should map Hex to code icon', () => {
    const entry = createMockHistoryEntry(FileItemType.Hex, 256, 'test.hex', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('code');
  });

  it('should map Unknown to insert_drive_file icon', () => {
    const entry = createMockHistoryEntry(FileItemType.Unknown, 128, 'test.dat', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();
    expect(component.fileIcon()).toBe('insert_drive_file');
  });

  it('should emit entrySelected on click', () => {
    const entry = createMockHistoryEntry(FileItemType.Song, 1024, 'test.sid', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();

    let emittedEntry: HistoryEntry | undefined;
    component.entrySelected.subscribe((e) => {
      emittedEntry = e;
    });

    const storageItemElement = fixture.nativeElement.querySelector('lib-storage-item');
    storageItemElement.click();

    expect(emittedEntry).toEqual(entry);
  });

  it('should emit entryDoubleClick on double click', () => {
    const entry = createMockHistoryEntry(FileItemType.Game, 2048, 'test.prg', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.detectChanges();

    let emittedEntry: HistoryEntry | undefined;
    component.entryDoubleClick.subscribe((e) => {
      emittedEntry = e;
    });

    const storageItemElement = fixture.nativeElement.querySelector('lib-storage-item');
    storageItemElement.dispatchEvent(new MouseEvent('dblclick'));

    expect(emittedEntry).toEqual(entry);
  });

  it('should apply selected class when selected is true', () => {
    const entry = createMockHistoryEntry(FileItemType.Song, 1024, 'test.sid', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.componentRef.setInput('selected', true);
    fixture.detectChanges();

    const storageItemElement = fixture.nativeElement.querySelector('lib-storage-item');
    expect(storageItemElement.classList.contains('selected')).toBe(true);
  });

  it('should not apply selected class when selected is false', () => {
    const entry = createMockHistoryEntry(FileItemType.Song, 1024, 'test.sid', Date.now());
    fixture.componentRef.setInput('entry', entry);
    fixture.componentRef.setInput('selected', false);
    fixture.detectChanges();

    const storageItemElement = fixture.nativeElement.querySelector('lib-storage-item');
    expect(storageItemElement.classList.contains('selected')).toBe(false);
  });
});

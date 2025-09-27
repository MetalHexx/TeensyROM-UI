import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DirectoryItemComponent } from './directory-item.component';
import { DirectoryItem } from '@teensyrom-nx/domain';

describe('DirectoryItemComponent', () => {
  let component: DirectoryItemComponent;
  let fixture: ComponentFixture<DirectoryItemComponent>;

  const mockDirectoryItem: DirectoryItem = {
    name: 'Test Directory',
    path: '/test/path',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryItemComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryItemComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('directoryItem', mockDirectoryItem);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render directory name', () => {
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Test Directory');
  });

  it('should emit itemSelected on single click', () => {
    let emittedItem: DirectoryItem | undefined;
    component.itemSelected.subscribe((item) => {
      emittedItem = item;
    });

    const directoryElement = fixture.nativeElement.querySelector('.directory-item');
    directoryElement.click();

    expect(emittedItem).toEqual(mockDirectoryItem);
  });

  it('should emit itemDoubleClicked on double click', () => {
    let emittedItem: DirectoryItem | undefined;
    component.itemDoubleClicked.subscribe((item) => {
      emittedItem = item;
    });

    const directoryElement = fixture.nativeElement.querySelector('.directory-item');
    directoryElement.dispatchEvent(new MouseEvent('dblclick'));

    expect(emittedItem).toEqual(mockDirectoryItem);
  });

  it('should apply selected class when selected is true', () => {
    fixture.componentRef.setInput('selected', true);
    fixture.detectChanges();

    const directoryElement = fixture.nativeElement.querySelector('.directory-item');
    expect(directoryElement.classList.contains('selected')).toBe(true);
  });

  it('should not apply selected class when selected is false', () => {
    fixture.componentRef.setInput('selected', false);
    fixture.detectChanges();

    const directoryElement = fixture.nativeElement.querySelector('.directory-item');
    expect(directoryElement.classList.contains('selected')).toBe(false);
  });
});

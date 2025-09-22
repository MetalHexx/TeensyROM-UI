import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DirectoryNavigateComponent } from './directory-navigate.component';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';

describe('DirectoryNavigateComponent', () => {
  let component: DirectoryNavigateComponent;
  let fixture: ComponentFixture<DirectoryNavigateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DirectoryNavigateComponent, IconButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(DirectoryNavigateComponent);
    component = fixture.componentInstance;
  });

  describe('Component Initialization', () => {
    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have default input values', () => {
      expect(component.canNavigateUp()).toBe(false);
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Input Properties', () => {
    it('should update canNavigateUp input', () => {
      fixture.componentRef.setInput('canNavigateUp', true);
      expect(component.canNavigateUp()).toBe(true);

      fixture.componentRef.setInput('canNavigateUp', false);
      expect(component.canNavigateUp()).toBe(false);
    });

    it('should update isLoading input', () => {
      fixture.componentRef.setInput('isLoading', true);
      expect(component.isLoading()).toBe(true);

      fixture.componentRef.setInput('isLoading', false);
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Event Handler Methods', () => {
    it('should emit backClicked when onBackClick is called', () => {
      let emitted = false;
      component.backClicked.subscribe(() => (emitted = true));

      component.onBackClick();

      expect(emitted).toBe(true);
    });

    it('should emit forwardClicked when onForwardClick is called', () => {
      let emitted = false;
      component.forwardClicked.subscribe(() => (emitted = true));

      component.onForwardClick();

      expect(emitted).toBe(true);
    });

    it('should emit upClicked when onUpClick is called', () => {
      let emitted = false;
      component.upClicked.subscribe(() => (emitted = true));

      component.onUpClick();

      expect(emitted).toBe(true);
    });

    it('should emit refreshClicked when onRefreshClick is called', () => {
      let emitted = false;
      component.refreshClicked.subscribe(() => (emitted = true));

      component.onRefreshClick();

      expect(emitted).toBe(true);
    });
  });
});

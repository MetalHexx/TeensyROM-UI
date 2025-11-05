import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { By } from '@angular/platform-browser';
import { RandomRollButtonComponent } from './random-roll-button.component';

const TEST_CONSTANTS = {
  CSS_CLASSES: {
    DICE_ROLL: 'dice-roll',
  },
  COLORS: {
    NORMAL: 'normal',
    ERROR: 'error',
    HIGHLIGHT: 'highlight',
  },
  SELECTORS: {
    ICON_BUTTON: 'lib-icon-button',
    MAT_ICON: 'mat-icon',
  },
  INPUT_NAMES: {
    COLOR: 'color',
  },
} as const;

describe('RandomRollButtonComponent', () => {
  let component: RandomRollButtonComponent;
  let fixture: ComponentFixture<RandomRollButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideNoopAnimations()],
      imports: [RandomRollButtonComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(RandomRollButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit buttonClick when onButtonClick is called', () => {
    const emitSpy = vitest.spyOn(component.buttonClick, 'emit');

    component.onButtonClick();

    expect(emitSpy).toHaveBeenCalled();
  });

  it('should add dice-roll class to mat-icon on animateDiceRoll', () => {
    const mockMatIcon = document.createElement(TEST_CONSTANTS.SELECTORS.MAT_ICON);
    const mockEvent = {
      target: {
        parentElement: {
          querySelector: vitest.fn().mockReturnValue(mockMatIcon),
        },
      },
    } as unknown as Event;

    component.animateDiceRoll(mockEvent);

    expect(mockMatIcon.classList.contains(TEST_CONSTANTS.CSS_CLASSES.DICE_ROLL)).toBe(true);
  });

  describe('Phase 4: Error State Visual Feedback', () => {
    it('should default to normal color', () => {
      expect(component.getButtonColor()).toBe(TEST_CONSTANTS.COLORS.NORMAL);
    });

    it('should accept error color input', () => {
      fixture.componentRef.setInput('getButtonColor', TEST_CONSTANTS.COLORS.ERROR);
      fixture.detectChanges();

      expect(component.getButtonColor()).toBe(TEST_CONSTANTS.COLORS.ERROR);
    });

    it('should pass color to icon-button component', () => {
      fixture.componentRef.setInput('getButtonColor', TEST_CONSTANTS.COLORS.ERROR);
      fixture.detectChanges();

      const iconButton = fixture.debugElement.query(By.css(TEST_CONSTANTS.SELECTORS.ICON_BUTTON));
      expect(iconButton.componentInstance.color()).toBe(TEST_CONSTANTS.COLORS.ERROR);
    });

    it('should update color when changed', () => {
      fixture.componentRef.setInput('getButtonColor', TEST_CONSTANTS.COLORS.NORMAL);
      fixture.detectChanges();
      expect(component.getButtonColor()).toBe(TEST_CONSTANTS.COLORS.NORMAL);

      fixture.componentRef.setInput('getButtonColor', TEST_CONSTANTS.COLORS.ERROR);
      fixture.detectChanges();
      expect(component.getButtonColor()).toBe(TEST_CONSTANTS.COLORS.ERROR);

      fixture.componentRef.setInput('getButtonColor', TEST_CONSTANTS.COLORS.HIGHLIGHT);
      fixture.detectChanges();
      expect(component.getButtonColor()).toBe(TEST_CONSTANTS.COLORS.HIGHLIGHT);
    });
  });
});

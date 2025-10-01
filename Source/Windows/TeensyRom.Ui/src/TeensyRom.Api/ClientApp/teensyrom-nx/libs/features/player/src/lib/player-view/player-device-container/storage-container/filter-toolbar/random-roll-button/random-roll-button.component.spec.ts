import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RandomRollButtonComponent } from './random-roll-button.component';

describe('RandomRollButtonComponent', () => {
  let component: RandomRollButtonComponent;
  let fixture: ComponentFixture<RandomRollButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RandomRollButtonComponent]
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
    const mockMatIcon = document.createElement('mat-icon');
    const mockEvent = {
      target: {
        parentElement: {
          querySelector: vitest.fn().mockReturnValue(mockMatIcon)
        }
      }
    } as unknown as Event;

    component.animateDiceRoll(mockEvent);

    expect(mockMatIcon.classList.contains('dice-roll')).toBe(true);
  });
});
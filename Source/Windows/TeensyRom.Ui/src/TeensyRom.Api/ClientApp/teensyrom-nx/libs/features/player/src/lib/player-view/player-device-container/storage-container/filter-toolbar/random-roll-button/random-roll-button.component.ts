import { Component, output, input } from '@angular/core';
import { IconButtonComponent, IconButtonColor } from '@teensyrom-nx/ui/components';

const COMPONENT_CONSTANTS = {
  CSS_CLASSES: {
    DICE_ROLL: 'dice-roll'
  },
  SELECTORS: {
    MAT_ICON: 'mat-icon'
  },
  COLORS: {
    NORMAL: 'normal'
  },
  ANIMATION: {
    DURATION_MS: 1200
  }
} as const;

@Component({
  selector: 'lib-random-roll-button',
  standalone: true,
  imports: [IconButtonComponent],
  templateUrl: './random-roll-button.component.html',
  styleUrl: './random-roll-button.component.scss'
})
export class RandomRollButtonComponent {
  getButtonColor = input<IconButtonColor>(COMPONENT_CONSTANTS.COLORS.NORMAL as IconButtonColor);
  buttonClick = output<void>();

  onButtonClick(): void {
    this.buttonClick.emit();
  }

  animateDiceRoll(event: Event): void {
    // Find the mat-icon element within the clicked element
    const target = event.target as HTMLElement;
    const matIcon = target.parentElement?.querySelector(COMPONENT_CONSTANTS.SELECTORS.MAT_ICON) || target.querySelector(COMPONENT_CONSTANTS.SELECTORS.MAT_ICON);
    
    console.log('Animation target found:', matIcon);
    
    if (matIcon) {
      // Remove existing animation class if present
      matIcon.classList.remove(COMPONENT_CONSTANTS.CSS_CLASSES.DICE_ROLL);
      
      // Force reflow to ensure the class removal takes effect
      void (matIcon as HTMLElement).offsetHeight;
      
      // Add the animation class
      matIcon.classList.add(COMPONENT_CONSTANTS.CSS_CLASSES.DICE_ROLL);
      
      // Remove the class after animation completes
      setTimeout(() => {
        matIcon.classList.remove(COMPONENT_CONSTANTS.CSS_CLASSES.DICE_ROLL);
      }, COMPONENT_CONSTANTS.ANIMATION.DURATION_MS);
    }
  }
}
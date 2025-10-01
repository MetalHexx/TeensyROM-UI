import { Component, output } from '@angular/core';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-random-roll-button',
  standalone: true,
  imports: [IconButtonComponent],
  templateUrl: './random-roll-button.component.html',
  styleUrl: './random-roll-button.component.scss'
})
export class RandomRollButtonComponent {
  buttonClick = output<void>();

  onButtonClick(): void {
    this.buttonClick.emit();
  }

  animateDiceRoll(event: Event): void {
    // Find the mat-icon element within the clicked element
    const target = event.target as HTMLElement;
    const matIcon = target.parentElement?.querySelector('mat-icon') || target.querySelector('mat-icon');
    
    console.log('Animation target found:', matIcon);
    
    if (matIcon) {
      // Remove existing animation class if present
      matIcon.classList.remove('dice-roll');
      
      // Force reflow to ensure the class removal takes effect
      void (matIcon as HTMLElement).offsetHeight;
      
      // Add the animation class
      matIcon.classList.add('dice-roll');
      
      // Remove the class after animation completes
      setTimeout(() => {
        matIcon.classList.remove('dice-roll');
      }, 1200); // Match the animation duration
    }
  }
}
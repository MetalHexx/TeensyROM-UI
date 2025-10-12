import { Component, input, output, signal, effect, Self, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import type { AnimationParentMode } from '../shared/animation.types';
import { PARENT_ANIMATION_COMPLETE } from '../shared/animation-tokens';

@Component({
  selector: 'lib-leet-text-container',
  imports: [CommonModule],
  templateUrl: './leet-text-container.component.html',
  styleUrl: './leet-text-container.component.scss',
  host: {
    '[class.animating]': 'isAnimating()'
  },
  providers: [
    {
      provide: PARENT_ANIMATION_COMPLETE,
      useFactory: (self: LeetTextContainerComponent) => {
        return self.animationCompleteSignal.asReadonly();
      },
      deps: [[new Self(), LeetTextContainerComponent]]
    }
  ]
})
export class LeetTextContainerComponent implements AfterViewInit {
  /**
   * Animation trigger - controls fade in/out and leet animation
   * - true: Fade in and run leet animation
   * - false: Fade out
   * - undefined: Render immediately without animation
   */
  animationTrigger = input<boolean | undefined>(undefined);

  /**
   * Duration of the leet cycling animation in milliseconds (default: 1000ms)
   */
  animationDuration = input<number>(1000);

  /**
   * Controls whether this component waits for parent animations
   * - undefined (default): No waiting - component animates immediately
   * - 'auto': Opt-in to wait for nearest animation parent in DI tree
   * - AnimationParent: Wait for specific component
   */
  animationParent = input<AnimationParentMode>(undefined);

  /**
   * Show spinner before the text (default: false)
   */
  showFrontSpinner = input<boolean>(false);

  /**
   * Show spinner after the text (default: false)
   */
  showBackSpinner = input<boolean>(false);

  /**
   * Emitted when both fade and leet animations complete
   */
  animationComplete = output<void>();

  // Internal animation completion signal for child components (public for provider access)
  animationCompleteSignal = signal(false);

  // Track if the leet animation is running
  protected isAnimating = signal(false);

  // The currently displayed text (may have leet characters during animation)
  protected displayText = signal('');

  // Spinner character for the animation
  protected spinnerChar = signal('');


  // Leet character mappings
  private leetMap: Record<string, string[]> = {
    'a': ['@', '4', 'a'],
    'e': ['3', 'e'],
    'i': ['1', '!', 'i'],
    'o': ['0', 'o'],
    's': ['5', '$', 's'],
    't': ['7', '+', 't'],
    'l': ['1', '|', 'l'],
    'g': ['9', '6', 'g'],
    'b': ['8', 'b'],
  };

  // Spinner animation characters
  private spinnerChars = ['/', '-', '\\', '|'];
  private spinnerIndex = 0;
  private spinnerInterval: any;

  // Store text content from ng-content
  private textContent = signal<string>('');

  constructor(private elementRef: ElementRef) {
    // Start spinner animation
    this.startSpinner();
  }

  ngAfterViewInit(): void {
    // Extract text content from ng-content (find the hidden div)
    const hiddenDiv = this.elementRef.nativeElement.querySelector('div[style*="display: none"]');
    const content = hiddenDiv?.textContent?.trim() || '';
    this.textContent.set(content);

    // Start continuous cycling animation
    if (content) {
      this.startContinuousCycling(content);
    }
  }

  private startContinuousCycling(text: string): void {
    // Build list of indices that have leet mappings
    const translatableIndices: number[] = [];
    text.split('').forEach((char, index) => {
      const lowerChar = char.toLowerCase();
      if (this.leetMap[lowerChar]) {
        translatableIndices.push(index);
      }
    });

    if (translatableIndices.length === 0) {
      // No translatable characters, just show original text
      this.displayText.set(text);
      return;
    }

    // Cycle through leet characters continuously, back and forth
    let currentPosition = 0; // Position in translatableIndices array
    let direction = 1; // 1 = forward, -1 = backward

    const cycleInterval = setInterval(() => {
      const charIndex = translatableIndices[currentPosition];

      const result = text.split('').map((char, index) => {
        const lowerChar = char.toLowerCase();

        // Check if this character has leet mappings
        if (this.leetMap[lowerChar]) {
          const options = this.leetMap[lowerChar];

          // If this is the current cycling position, show leet character
          if (index === charIndex) {
            const leetChar = options[Math.floor(Math.random() * (options.length - 1))];
            return char === char.toUpperCase() ? leetChar.toUpperCase() : leetChar;
          }
        }

        return char;
      }).join('');

      this.displayText.set(result);

      // Move to next translatable character position
      currentPosition += direction;

      // Reverse direction at boundaries
      if (currentPosition >= translatableIndices.length - 1) {
        direction = -1; // Start going backward
      } else if (currentPosition <= 0) {
        direction = 1; // Start going forward
      }
    }, 200); // Cycle every 200ms

    // Store interval for cleanup
    (this as any).cycleInterval = cycleInterval;
  }

  ngOnDestroy(): void {
    // Clean up spinner interval
    if (this.spinnerInterval) {
      clearInterval(this.spinnerInterval);
    }
    // Clean up leet animation interval
    if ((this as any).leetAnimationInterval) {
      clearInterval((this as any).leetAnimationInterval);
    }
    // Clean up cycle interval
    if ((this as any).cycleInterval) {
      clearInterval((this as any).cycleInterval);
    }
  }

  private startSpinner(): void {
    // Update spinner character every 100ms
    this.spinnerInterval = setInterval(() => {
      this.spinnerChar.set(this.spinnerChars[this.spinnerIndex]);
      this.spinnerIndex = (this.spinnerIndex + 1) % this.spinnerChars.length;
    }, 100);
  }


  private startLeetAnimation(finalText: string): void {
    this.isAnimating.set(true);
    const duration = this.animationDuration();
    const steps = 30; // Number of animation frames per direction
    const interval = duration / steps;
    let currentStep = 0;
    let direction = -1; // Start going from leet to normal (-1), then reverse to normal to leet (1)

    const animationInterval = setInterval(() => {
      currentStep++;

      // Calculate progress (0 to 1)
      const cycleProgress = (currentStep % steps) / steps;

      // Determine leet amount based on direction
      let leetAmount: number;
      if (direction === -1) {
        // Going from leet (1) to normal (0)
        leetAmount = 1 - cycleProgress;
      } else {
        // Going from normal (0) to leet (1)
        leetAmount = cycleProgress;
      }

      // Generate text with current leet amount
      const animatedText = this.generateLeetText(finalText, leetAmount);
      this.displayText.set(animatedText);

      // Reverse direction when we complete a cycle
      if (currentStep % steps === 0 && currentStep > 0) {
        direction *= -1; // Flip direction
      }
    }, interval);

    // Store interval for cleanup
    (this as any).leetAnimationInterval = animationInterval;
  }

  private generateLeetText(text: string, leetAmount: number): string {
    return text.split('').map((char, index) => {
      const lowerChar = char.toLowerCase();

      // Check if this character has a leet mapping
      if (this.leetMap[lowerChar]) {
        const options = this.leetMap[lowerChar];

        // Calculate if we should use leet for this character based on leetAmount
        // Use deterministic pattern based on character position for consistency
        const shouldLeet = Math.random() < leetAmount;

        if (shouldLeet) {
          // Pick a random leet character (excluding the last option which is the original)
          const leetChar = options[Math.floor(Math.random() * (options.length - 1))];
          return char === char.toUpperCase() ? leetChar.toUpperCase() : leetChar;
        }
      }

      return char;
    }).join('');
  }
}

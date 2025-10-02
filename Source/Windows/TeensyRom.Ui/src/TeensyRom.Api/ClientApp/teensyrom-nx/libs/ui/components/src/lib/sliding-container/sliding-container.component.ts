import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, style, transition, animate } from '@angular/animations';
import type { AnimationDirection } from '../shared/animation.types';

export type ContainerAnimationDirection = 
  | AnimationDirection
  | 'slide-down'
  | 'slide-up'
  | 'fade';

@Component({
  selector: 'lib-sliding-container',
  imports: [CommonModule],
  templateUrl: './sliding-container.component.html',
  styleUrl: './sliding-container.component.scss',
  animations: [
    trigger('containerAnimation', [
      transition(':enter', [
        style({
          opacity: 0,
          height: '{{ startHeight }}',
          width: '{{ startWidth }}',
          overflow: 'hidden',
          transform: '{{ startTransform }}'
        }),
        animate('{{ duration }}ms {{ easing }}', style({
          opacity: 1,
          height: '{{ endHeight }}',
          width: '{{ endWidth }}',
          overflow: 'visible',
          transform: '{{ endTransform }}'
        }))
      ], { params: { 
        startHeight: '0', 
        endHeight: 'auto',
        startWidth: 'auto',
        endWidth: 'auto',
        startTransform: 'translateY(-20px)', 
        endTransform: 'translateY(0)',
        duration: '400',
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }}),
      transition(':leave', [
        animate('{{ duration }}ms {{ easing }}', style({
          opacity: 0,
          height: '{{ startHeight }}',
          width: '{{ startWidth }}',
          overflow: 'hidden',
          transform: '{{ startTransform }}'
        }))
      ], { params: { 
        startHeight: '0',
        startWidth: 'auto',
        startTransform: 'translateY(-20px)',
        duration: '400',
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }})
    ])
  ]
})
export class SlidingContainerComponent {
  // Animation configuration inputs
  containerHeight = input<string>('auto'); // Height of the animated container
  containerWidth = input<string>('auto'); // Width of the animated container
  animationDuration = input<number>(400); // Duration of container animation
  animationDirection = input<ContainerAnimationDirection>('from-top'); // Animation direction

  // Output events
  containerAnimationComplete = output<void>(); // Emitted when container animation finishes

  // The AnimatedContainer now focuses just on its own animation
  // Content timing is handled by parent components via events

  // Animation parameter computation
  get animationParams() {
    const direction = this.animationDirection();
    const duration = this.animationDuration();
    const height = this.containerHeight();
    const width = this.containerWidth();
    const { startHeight, endHeight, startWidth, endWidth, startTransform, endTransform } = this.getAnimationValues(direction, height, width);

    return {
      value: 'visible',
      params: {
        startHeight,
        endHeight,
        startWidth,
        endWidth,
        startTransform,
        endTransform,
        duration: duration.toString(),
        easing: 'cubic-bezier(0.4, 0.0, 0.2, 1)'
      }
    };
  }

  onContainerAnimationDone(): void {
    // Emit event when container animation completes
    this.containerAnimationComplete.emit();
  }

  private getAnimationValues(direction: ContainerAnimationDirection, height: string, width: string): {
    startHeight: string;
    endHeight: string;
    startWidth: string;
    endWidth: string;
    startTransform: string;
    endTransform: string;
  } {
    switch (direction) {
      case 'fade':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(0, 0)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'slide-down':
      case 'from-top':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateY(-20px)',
          endTransform: 'translateY(0)'
        };
      
      case 'slide-up':
      case 'from-bottom':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateY(20px)',
          endTransform: 'translateY(0)'
        };
      
      case 'from-left':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateX(-20px)',
          endTransform: 'translateX(0)'
        };
      
      case 'from-right':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translateX(20px)',
          endTransform: 'translateX(0)'
        };
      
      case 'from-top-left':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(-20px, -20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-top-right':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(20px, -20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-bottom-left':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(-20px, 20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'from-bottom-right':
        return {
          startHeight: '0',
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(20px, 20px)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'none':
        return {
          startHeight: height,
          endHeight: height,
          startWidth: width,
          endWidth: width,
          startTransform: 'translate(0, 0)',
          endTransform: 'translate(0, 0)'
        };
      
      case 'random': {
        const directions: ContainerAnimationDirection[] = [
          'from-left', 'from-right', 'from-top', 'from-bottom',
          'from-top-left', 'from-top-right', 'from-bottom-left', 'from-bottom-right'
        ];
        const randomDir = directions[Math.floor(Math.random() * directions.length)];
        return this.getAnimationValues(randomDir, height, width);
      }
      
      default:
        return this.getAnimationValues('from-top', height, width);
    }
  }
}
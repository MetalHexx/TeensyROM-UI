import { Component, input, output } from '@angular/core';
import { IconButtonComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-directory-navigate',
  standalone: true,
  imports: [IconButtonComponent],
  templateUrl: './directory-navigate.component.html',
  styleUrl: './directory-navigate.component.scss',
})
export class DirectoryNavigateComponent {
  // Inputs
  canNavigateUp = input<boolean>(false);
  canNavigateBack = input<boolean>(false);
  canNavigateForward = input<boolean>(false);
  isLoading = input<boolean>(false);

  // Outputs
  backClicked = output<void>();
  forwardClicked = output<void>();
  upClicked = output<void>();
  refreshClicked = output<void>();

  // Event handlers
  onBackClick(): void {
    this.backClicked.emit();
  }

  onForwardClick(): void {
    this.forwardClicked.emit();
  }

  onUpClick(): void {
    this.upClicked.emit();
  }

  onRefreshClick(): void {
    this.refreshClicked.emit();
  }
}

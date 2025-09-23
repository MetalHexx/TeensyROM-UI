import { Component, input, output, computed } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';

interface PathSegment {
  label: string;
  path: string;
}

@Component({
  selector: 'lib-directory-breadcrumb',
  standalone: true,
  imports: [MatChipsModule],
  templateUrl: './directory-breadcrumb.component.html',
  styleUrl: './directory-breadcrumb.component.scss',
})
export class DirectoryBreadcrumbComponent {
  // Inputs
  currentPath = input.required<string>();
  storageType = input.required<string>();

  // Outputs
  navigationRequested = output<string>();

  // Computed segments
  pathSegments = computed(() => this.calculatePathSegments());

  onChipClick(path: string): void {
    this.navigationRequested.emit(path);
  }

  private calculatePathSegments(): PathSegment[] {
    const path = this.currentPath();
    const storageTypeLabel = this.storageType();

    // Handle root case
    if (path === '/' || path === '') {
      return [{ label: storageTypeLabel, path: '/' }];
    }

    const segments: PathSegment[] = [{ label: storageTypeLabel, path: '/' }];

    // Split path and build cumulative segments
    const pathParts = path.split('/').filter((part) => part.length > 0);
    let currentPath = '';

    for (const part of pathParts) {
      currentPath += '/' + part;
      segments.push({
        label: part,
        path: currentPath,
      });
    }

    return segments;
  }
}

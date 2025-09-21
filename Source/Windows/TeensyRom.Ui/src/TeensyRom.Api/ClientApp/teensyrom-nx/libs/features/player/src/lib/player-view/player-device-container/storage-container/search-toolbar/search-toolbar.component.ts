import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent, InputFieldComponent } from '@teensyrom-nx/ui/components';

@Component({
  selector: 'lib-search-toolbar',
  imports: [CommonModule, CompactCardLayoutComponent, InputFieldComponent],
  templateUrl: './search-toolbar.component.html',
  styleUrl: './search-toolbar.component.scss',
})
export class SearchToolbarComponent {}

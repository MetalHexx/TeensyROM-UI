import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CompactCardLayoutComponent } from '@teensyrom-nx/ui/components';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'lib-search-toolbar',
  imports: [
    CommonModule,
    CompactCardLayoutComponent,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
  ],
  templateUrl: './search-toolbar.component.html',
  styleUrl: './search-toolbar.component.scss',
})
export class SearchToolbarComponent {}

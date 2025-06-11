import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'lib-busy-dialog',
  standalone: true,
  imports: [MatDialogModule, MatProgressBarModule],
  templateUrl: './busy-dialog.component.html',
  styleUrls: ['./busy-dialog.component.scss'],
})
export class BusyDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: {
      message: string;
      title: string;
    }
  ) {}
}

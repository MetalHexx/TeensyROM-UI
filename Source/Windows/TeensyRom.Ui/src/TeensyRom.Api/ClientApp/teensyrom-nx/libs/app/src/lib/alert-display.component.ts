import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AlertMessage, AlertSeverity } from '@teensyrom-nx/domain';

@Component({
  selector: 'lib-alert-display',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  templateUrl: './alert-display.component.html',
  styleUrl: './alert-display.component.scss',
})
export class AlertDisplayComponent {
  alert = input.required<AlertMessage>();
  dismissed = output<string>();

  iconName = computed(() => {
    const severity = this.alert().severity;
    switch (severity) {
      case AlertSeverity.Success:
        return 'check_circle';
      case AlertSeverity.Error:
        return 'error';
      case AlertSeverity.Warning:
        return 'warning';
      case AlertSeverity.Info:
        return 'info';
      default:
        return 'info';
    }
  });

  onDismiss(): void {
    this.dismissed.emit(this.alert().id);
  }
}

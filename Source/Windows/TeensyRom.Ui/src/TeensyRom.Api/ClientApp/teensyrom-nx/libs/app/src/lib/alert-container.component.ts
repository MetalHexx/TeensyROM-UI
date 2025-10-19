import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ALERT_SERVICE, AlertPosition, AlertMessage } from '@teensyrom-nx/domain';
import { toSignal } from '@angular/core/rxjs-interop';
import { AlertDisplayComponent } from './alert-display.component';

interface PositionGroup {
  [key: string]: AlertMessage[];
}

@Component({
  selector: 'lib-alert-container',
  standalone: true,
  imports: [CommonModule, AlertDisplayComponent],
  templateUrl: './alert-container.component.html',
  styleUrl: './alert-container.component.scss',
})
export class AlertContainerComponent {
  private alertService = inject(ALERT_SERVICE);
  private alertsSignal = toSignal(this.alertService.alerts$, { initialValue: [] });

  alertsByPosition = computed(() => {
    const alerts = this.alertsSignal();
    const grouped: PositionGroup = {};

    // Initialize all position groups
    Object.values(AlertPosition).forEach((position) => {
      grouped[position] = [];
    });

    // Group alerts by position
    alerts.forEach((alert) => {
      grouped[alert.position].push(alert);
    });

    return grouped;
  });

  positions = Object.values(AlertPosition);

  onAlertDismissed(alertId: string): void {
    this.alertService.dismiss(alertId);
  }
}

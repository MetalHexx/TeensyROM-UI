import { Injectable, signal } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { IAlertService, AlertMessage, AlertSeverity, AlertPosition } from '@teensyrom-nx/domain';

const DEFAULT_AUTO_DISMISS_MS = 5000;
const DEFAULT_POSITION = AlertPosition.BottomCenter;

@Injectable()
export class AlertService implements IAlertService {
  private alertsSignal = signal<AlertMessage[]>([]);
  private timerMap = new Map<string, number>();
  private alertsSubject$ = new BehaviorSubject<AlertMessage[]>([]);

  alerts$ = this.alertsSubject$.asObservable();

  private updateAlerts(alerts: AlertMessage[]): void {
    this.alertsSignal.set(alerts);
    this.alertsSubject$.next(alerts);
  }

  show(
    message: string,
    severity: AlertSeverity,
    position: AlertPosition = DEFAULT_POSITION,
    autoDismissMs: number = DEFAULT_AUTO_DISMISS_MS
  ): void {
    const id = crypto.randomUUID();
    const alert: AlertMessage = {
      id,
      message,
      severity,
      position,
      autoDismissMs,
    };

    const updatedAlerts = [...this.alertsSignal(), alert];
    this.updateAlerts(updatedAlerts);

    // Start auto-dismiss timer
    const timerId = window.setTimeout(() => {
      this.dismiss(id);
    }, autoDismissMs);

    this.timerMap.set(id, timerId);
  }

  success(
    message: string,
    position: AlertPosition = DEFAULT_POSITION,
    autoDismissMs?: number
  ): void {
    this.show(message, AlertSeverity.Success, position, autoDismissMs);
  }

  error(
    message: string,
    position: AlertPosition = DEFAULT_POSITION,
    autoDismissMs?: number
  ): void {
    this.show(message, AlertSeverity.Error, position, autoDismissMs);
  }

  warning(
    message: string,
    position: AlertPosition = DEFAULT_POSITION,
    autoDismissMs?: number
  ): void {
    this.show(message, AlertSeverity.Warning, position, autoDismissMs);
  }

  info(
    message: string,
    position: AlertPosition = DEFAULT_POSITION,
    autoDismissMs?: number
  ): void {
    this.show(message, AlertSeverity.Info, position, autoDismissMs);
  }

  dismiss(alertId: string): void {
    // Cancel auto-dismiss timer if it exists
    const timerId = this.timerMap.get(alertId);
    if (timerId !== undefined) {
      clearTimeout(timerId);
      this.timerMap.delete(alertId);
    }

    // Remove alert from signal and subject
    const updatedAlerts = this.alertsSignal().filter((a) => a.id !== alertId);
    this.updateAlerts(updatedAlerts);
  }
}

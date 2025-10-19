import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { AlertMessage } from '../models/alert-message.model';
import { AlertSeverity } from '../models/alert-severity.enum';
import { AlertPosition } from '../models/alert-position.enum';

export interface IAlertService {
  alerts$: Observable<AlertMessage[]>;
  show(
    message: string,
    severity: AlertSeverity,
    position?: AlertPosition,
    autoDismissMs?: number
  ): void;
  success(message: string, position?: AlertPosition, autoDismissMs?: number): void;
  error(message: string, position?: AlertPosition, autoDismissMs?: number): void;
  warning(message: string, position?: AlertPosition, autoDismissMs?: number): void;
  info(message: string, position?: AlertPosition, autoDismissMs?: number): void;
  dismiss(alertId: string): void;
}

export const ALERT_SERVICE = new InjectionToken<IAlertService>('ALERT_SERVICE');

import { AlertSeverity } from './alert-severity.enum';
import { AlertPosition } from './alert-position.enum';

export interface AlertMessage {
  id: string;
  message: string;
  severity: AlertSeverity;
  position: AlertPosition;
  autoDismissMs?: number;
}

import { ALERT_SERVICE } from '@teensyrom-nx/domain';
import { AlertService } from './alert.service';

export const ALERT_SERVICE_PROVIDER = {
  provide: ALERT_SERVICE,
  useClass: AlertService,
};

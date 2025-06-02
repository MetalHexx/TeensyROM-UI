import {
  Component,
  ViewChild,
  ElementRef,
  AfterViewChecked,
  inject,
  signal,
  effect,
  EffectRef,
} from '@angular/core';
import { DeviceLogsService } from '@teensyrom-nx/domain/device/services';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'lib-device-logs',
  templateUrl: './device-logs.component.html',
  styleUrls: ['./device-logs.component.scss'],
  standalone: true,
  imports: [MatCardModule, MatIconModule, MatButtonModule],
})
export class DeviceLogsComponent {
  private readonly logsService = inject(DeviceLogsService);
  readonly logs = this.logsService.logs;
  readonly isConnected = signal(false);

  logEffectRef: EffectRef | undefined = effect(() => {
    const logs = this.logs();
    if (logs.length) {
      queueMicrotask(() => this.scrollToElement());
    }
  });

  @ViewChild('logsContent') logsContentRef!: ElementRef<HTMLDivElement>;

  startLogs() {
    this.logsService.connect();
    this.isConnected.set(true);
  }

  stopLogs() {
    this.logsService.disconnect();
    this.isConnected.set(false);
    if (this.logEffectRef) this.logEffectRef.destroy();
  }

  clearLogs() {
    this.logsService.clear();
  }

  scrollToElement(): void {
    this.logsContentRef.nativeElement.scroll({
      top: this.logsContentRef.nativeElement.scrollHeight,
      left: 0,
      behavior: 'smooth',
    });
  }
}

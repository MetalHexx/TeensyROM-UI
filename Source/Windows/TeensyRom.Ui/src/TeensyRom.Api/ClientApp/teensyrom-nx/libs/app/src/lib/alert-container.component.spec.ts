import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AlertContainerComponent } from './alert-container.component';
import {
  ALERT_SERVICE,
  AlertPosition,
  AlertMessage,
  AlertSeverity,
  IAlertService,
} from '@teensyrom-nx/domain';
import { BehaviorSubject } from 'rxjs';
import { By } from '@angular/platform-browser';
import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('AlertContainerComponent', () => {
  let component: AlertContainerComponent;
  let fixture: ComponentFixture<AlertContainerComponent>;
  let mockAlertService: Partial<IAlertService>;
  let alertsSubject: BehaviorSubject<AlertMessage[]>;

  const mockAlerts: AlertMessage[] = [
    {
      id: 'alert-1',
      message: 'Error message',
      severity: AlertSeverity.Error,
      position: AlertPosition.BottomRight,
      autoDismissMs: 3000,
    },
    {
      id: 'alert-2',
      message: 'Success message',
      severity: AlertSeverity.Success,
      position: AlertPosition.BottomRight,
      autoDismissMs: 3000,
    },
  ];

  beforeEach(async () => {
    alertsSubject = new BehaviorSubject(mockAlerts);
    mockAlertService = {
      alerts$: alertsSubject.asObservable(),
      dismiss: vi.fn(),
    };

    await TestBed.configureTestingModule({
      imports: [AlertContainerComponent],
      providers: [{ provide: ALERT_SERVICE, useValue: mockAlertService }],
    }).compileComponents();

    fixture = TestBed.createComponent(AlertContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should render alert container', () => {
    const container = fixture.debugElement.query(By.css('.alert-container'));
    expect(container).toBeTruthy();
  });

  it('should render all 6 position containers', () => {
    const positions = fixture.debugElement.queryAll(By.css('[data-position]'));
    expect(positions.length).toBe(6);
  });

  it('should render alerts from service', () => {
    const displayComponents = fixture.debugElement.queryAll(By.css('lib-alert-display'));
    expect(displayComponents.length).toBe(2);
  });

  it('should group alerts by position', () => {
    const alertsByPos = component.alertsByPosition();
    expect(alertsByPos[AlertPosition.BottomRight].length).toBe(2);
    expect(alertsByPos[AlertPosition.TopLeft].length).toBe(0);
  });

  it('should call dismiss service when alert dismissed', () => {
    component.onAlertDismissed('alert-1');
    expect(mockAlertService.dismiss).toHaveBeenCalledWith('alert-1');
  });

  it('should update reactively when alerts change', () => {
    const newAlerts: AlertMessage[] = [
      {
        id: 'alert-3',
        message: 'New alert',
        severity: AlertSeverity.Info,
        position: AlertPosition.TopLeft,
        autoDismissMs: 3000,
      },
    ];

    alertsSubject.next(newAlerts);
    fixture.detectChanges();

    const alertsByPos = component.alertsByPosition();
    expect(alertsByPos[AlertPosition.TopLeft].length).toBe(1);
    expect(alertsByPos[AlertPosition.BottomRight].length).toBe(0);
  });

  it('should have correct position classes', () => {
    const topLeftDiv = fixture.debugElement.query(By.css('.position-top-left'));
    const bottomRightDiv = fixture.debugElement.query(By.css('.position-bottom-right'));

    expect(topLeftDiv).toBeTruthy();
    expect(bottomRightDiv).toBeTruthy();
  });
});

import { Routes } from '@angular/router';
import { ROUTES } from './core/services/navigation/navigation.constants';
import { LayoutComponent } from './shell/layout/layout.component';
import { PlayerViewComponent } from './features/player/player-view/player-view.component';
import { MixingViewComponent } from './features/mixing/mixing-view/mixing-view.component';
import { DevicesViewComponent } from './features/devices/device-view/devices-view.component';
import { SettingsViewComponent } from './features/settings/settings-view.component';
import { ThemeTesterComponent } from './features/theme-tester/theme-tester.component';

export const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: ROUTES.PLAYER,
        component: PlayerViewComponent,
      },
      {
        path: ROUTES.MIXER,
        component: MixingViewComponent,
      },
      {
        path: ROUTES.DEVICES,
        component: DevicesViewComponent,
      },
      {
        path: ROUTES.SETTINGS,
        component: SettingsViewComponent,
      },
      {
        path: ROUTES.THEME_TESTER,
        component: ThemeTesterComponent,
      },
      {
        path: '',
        redirectTo: ROUTES.THEME_TESTER,
        pathMatch: 'full',
      },
    ],
  },
];

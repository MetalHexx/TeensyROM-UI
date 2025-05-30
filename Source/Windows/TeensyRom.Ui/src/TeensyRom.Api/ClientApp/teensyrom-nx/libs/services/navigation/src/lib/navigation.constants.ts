// import { NavItem } from './nav-item.model'; // Commented out due to missing module

import { NavItem } from './navigation-item.model';

export const ROUTES = {
  PLAYER: 'jukebox',
  MIXER: 'mixing',
  DEVICES: 'devices',
  SETTINGS: 'settings',
  THEME_TESTER: 'themes',
} as const;

export const ROUTE_TITLES = {
  [ROUTES.PLAYER]: 'Player',
  [ROUTES.MIXER]: 'DJ Mixer',
  [ROUTES.DEVICES]: 'Devices',
  [ROUTES.SETTINGS]: 'Settings',
  [ROUTES.THEME_TESTER]: 'Theme Tester',
} as const;

export const NAV_ITEMS: NavItem[] = [
  {
    name: ROUTE_TITLES[ROUTES.PLAYER],
    icon: 'play_arrow',
    route: ROUTES.PLAYER,
  },
  {
    name: ROUTE_TITLES[ROUTES.MIXER],
    icon: 'tune',
    route: ROUTES.MIXER,
  },
  {
    name: ROUTE_TITLES[ROUTES.DEVICES],
    icon: 'devices',
    route: ROUTES.DEVICES,
  },
  {
    name: ROUTE_TITLES[ROUTES.SETTINGS],
    icon: 'settings',
    route: ROUTES.SETTINGS,
  },
  {
    name: ROUTE_TITLES[ROUTES.THEME_TESTER],
    icon: 'palette',
    route: ROUTES.THEME_TESTER,
  },
] as const;

import { NavItem } from './nav-item.model';

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
    payload: { route: ROUTES.PLAYER },
  },
  {
    name: ROUTE_TITLES[ROUTES.MIXER],
    icon: 'tune',
    route: ROUTES.MIXER,
    payload: { route: ROUTES.MIXER },
  },
  {
    name: ROUTE_TITLES[ROUTES.DEVICES],
    icon: 'devices',
    route: ROUTES.DEVICES,
    payload: { route: ROUTES.DEVICES },
  },
  {
    name: ROUTE_TITLES[ROUTES.SETTINGS],
    icon: 'settings',
    route: ROUTES.SETTINGS,
    payload: { route: ROUTES.SETTINGS },
  },
  {
    name: ROUTE_TITLES[ROUTES.THEME_TESTER],
    icon: 'palette',
    route: ROUTES.THEME_TESTER,
    payload: { route: ROUTES.THEME_TESTER },
  },
] as const;

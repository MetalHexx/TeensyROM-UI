export interface NavItem {
  name: string;
  icon: string;
  route: string;
  payload?: { route: string };
}

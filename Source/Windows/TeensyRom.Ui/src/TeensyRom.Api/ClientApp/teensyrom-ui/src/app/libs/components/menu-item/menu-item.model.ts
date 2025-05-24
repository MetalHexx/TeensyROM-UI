export interface MenuItem<T = unknown> {
  name: string;
  icon: string;
  data?: T;
}

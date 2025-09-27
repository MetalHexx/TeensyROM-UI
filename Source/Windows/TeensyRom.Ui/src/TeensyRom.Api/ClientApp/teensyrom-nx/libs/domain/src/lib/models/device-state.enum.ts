/**
 * Pure domain enum representing device connection states
 * Independent of any external API or infrastructure concerns
 */
export enum DeviceState {
  Connected = 'Connected',
  Connectable = 'Connectable', 
  ConnectionLost = 'ConnectionLost',
  Busy = 'Busy',
  Unknown = 'Unknown'
}
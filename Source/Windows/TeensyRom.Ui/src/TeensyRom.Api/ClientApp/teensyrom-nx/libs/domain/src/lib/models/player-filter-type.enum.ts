/**
 * Content filtering system for player operations
 * Determines which file types are considered for playback operations
 */
export enum PlayerFilterType {
  /** Any supported file type */
  All = 'ALL',
  
  /** Game files only */
  Games = 'GAMES',
  
  /** SID files only */
  Music = 'MUSIC',
  
  /** Image files only */
  Images = 'IMAGES',
  
  /** Hex files only */
  Hex = 'HEX'
}
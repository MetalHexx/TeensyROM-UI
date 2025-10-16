/**
 * Represents a link associated with a file (e.g., CSDB Profile, external resources)
 */
export interface FileLink {
  /** The display name of the link */
  name: string;
  /** The URL of the link */
  url: string;
}
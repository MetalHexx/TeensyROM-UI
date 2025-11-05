/**
 * Represents a competition entry associated with a file from DeepSID database
 */
export interface Competition {
  /** The name of the competition */
  name: string;
  /** The place/position achieved in the competition (optional) */
  place?: number;
}

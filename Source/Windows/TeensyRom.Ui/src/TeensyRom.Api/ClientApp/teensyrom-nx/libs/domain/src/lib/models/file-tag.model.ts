/**
 * Represents a tag associated with a file from DeepSID database
 */
export interface FileTag {
  /** The name of the tag */
  name: string;
  /** The type of the tag (e.g., genre, style, etc.) */
  type: string;
}
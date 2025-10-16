/**
 * Represents a YouTube video associated with a file from DeepSID database
 */
export interface YouTubeVideo {
  /** The YouTube video ID */
  videoId: string;
  /** The full URL to the YouTube video */
  url: string;
  /** The YouTube channel name */
  channel: string;
  /** The subtune number this video is associated with */
  subtune: number;
}
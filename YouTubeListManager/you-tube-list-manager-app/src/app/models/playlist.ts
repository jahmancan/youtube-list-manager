import { PlaylistItem } from "./playlist-item";
import { PrivacyStatus} from "./enums";

export class Playlist {
  Title: string;
  Privacy: PrivacyStatus = PrivacyStatus.Public;
  Hash: string ;
  ThumbnailUrl: string;
  ItemCount: number = 0;
  PlaylistItemsNextPageToken: string;

  PlaylistItems: PlaylistItem[];
}

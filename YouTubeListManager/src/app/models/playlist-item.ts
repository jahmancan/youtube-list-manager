import { VideoInfo} from "./video";
import { Playlist } from "./playlist";

export class PlaylistItem {
  Id: number;
  Hash: string;
  Position: number;
  VideoInfo: VideoInfo;
  Playlist: Playlist;
}

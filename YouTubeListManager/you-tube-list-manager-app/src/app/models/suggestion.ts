import { Playlist } from './playlist';
import { VideoInfo} from "./video";
import { PrivacyStatus } from "./enums";

export class Suggestion {

  playlist: Playlist;
  playlistStatus: PrivacyStatus;

  suggestions: VideoInfo[];
  currentIndex: number;
  current: Playlist;

  nextPageSuggestionsToken: string;
  playListItemsFetched: boolean = false;
  autoLoad: boolean = false;
  searchKey: string;
}

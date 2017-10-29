import { Component, OnInit }  from '@angular/core';
import { ActivatedRoute }     from '@angular/router';

import { Playlist }     from './models/playlist';
import { PlaylistItem } from "./models/playlist-item";
import { VideoInfo }    from "./models/video";
import { Suggestion }   from "./models/suggestion";

import { YouTubeDataService } from "./services/youtube-data-service";


@Component({
  selector: 'suggestions',
  templateUrl: './templates/suggestions.component.html',
  styleUrls: ['./styles/app.component.css']
})



export class SuggestionsComponent  implements OnInit {

  autoautoLoad: boolean = false;
  playlistId: string;
  playlist: Playlist;
  suggestions: VideoInfo[];
  current: PlaylistItem;
  markedItems: PlaylistItem[];

  constructor(private route:ActivatedRoute,
              private dataService: YouTubeDataService) { }

  ngOnInit(): void {
    this.playlistId = this.route.snapshot.params['playListId'];
    console.log(this.playlistId);
    if (!this.playlistId){
      return;
    }

    this.dataService.getPlaylist(this.playlistId)
      .then(playlist =>
        //console.log(playlist)
        this.playlist = playlist
      );
  }
}

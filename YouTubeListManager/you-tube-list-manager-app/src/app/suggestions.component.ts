import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Playlist } from './models/playlist';
import { PlaylistItem } from "./models/playlist-item";
import { Suggestion } from "./models/suggestion";

import { YouTubeDataService } from "./services/youtube-data-service";
import {VideoInfo} from "./models/video";

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
    this.playlistId = this.route.snapshot.params['id'];
    if (!this.playlistId){
      return;
    }
    console.log(this.playlistId);
    this.dataService.getPlaylist(this.playlistId)
      .then(playlist =>
        console.log(playlist)
        //this.playlist = playlist;
      );
  }
}

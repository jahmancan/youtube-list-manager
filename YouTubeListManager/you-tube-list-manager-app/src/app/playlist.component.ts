import { Component, OnInit } from '@angular/core';
import { Router }            from '@angular/router';

import { YouTubeDataService } from "./services/youtube-data-service";
import {Playlist} from "./models/playlist";

@Component({
  selector: 'playlist',
  templateUrl: './templates/playlists.component.html',
  styleUrls: ['./styles/app.component.css']
})

export class PlaylistComponent implements OnInit{

  playlists: Playlist[];

  constructor(private router: Router,
              private dataService: YouTubeDataService) { }

  selectPlayList(id: string): void {
    let link = ['/suggestions', id];
    this.router.navigate(link);
  }

  ngOnInit(): void {
    this.dataService.getPlaylists()
      .then(playlists => this.playlists = playlists);
  }
}

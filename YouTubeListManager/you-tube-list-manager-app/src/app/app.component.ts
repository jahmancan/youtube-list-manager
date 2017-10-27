import { Component, OnInit } from '@angular/core';
import { Router }            from '@angular/router';

import { YouTubeDataService } from "./services/youtube-data-service";
import {Playlist} from "./models/playlist";

@Component({
  selector: 'you-tube-list-manager',
  templateUrl: './templates/app.component.html',
  styleUrls: ['./styles/app.component.css']
})

export class AppComponent implements OnInit{

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

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
        this.getPlaylistItemsAsync(playlist)
      );
  }

  private getPlaylistItemsAsync(response: any): void {
    let responseItem = (undefined === response.Response) ? response : response.Response;

    let isInnerPageTokenPresent = responseItem.PlaylistItemsNextPageToken !== undefined;
    let nextPageToken = (isInnerPageTokenPresent)
      ? responseItem.PlaylistItemsNextPageToken
      : response.NextPageToken;

    if (isInnerPageTokenPresent) {
      this.playlist = responseItem;
      //this.playlistStatus = responseItem.PrivacyStatus;
    } else {
      console.log(response);
      this.playlist.PlaylistItems.concat(response.Response as PlaylistItem[]);
    }


    if (!nextPageToken) {
      return;
      //emit event for dragular
      //$scope.$broadcast('Playlistfetched');
    }


    console.log(nextPageToken);
    let nextResponse = this.dataService.getPlaylistItems(nextPageToken, this.playlist.Hash);
    this.getPlaylistItemsAsync(nextResponse);
  }
}

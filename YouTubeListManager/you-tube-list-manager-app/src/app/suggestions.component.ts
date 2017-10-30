import { Component, OnInit }  from '@angular/core';
import { ActivatedRoute }     from '@angular/router';

import { Observable }         from 'rxjs/Observable';
import { BehaviorSubject }    from 'rxjs/BehaviorSubject';

// Observable operators
import 'rxjs/add/operator/distinctUntilChanged';

import { Playlist }     from './models/playlist';
import { PlaylistItem } from './models/playlist-item';
import { VideoInfo }    from './models/video';
import { Suggestion }   from './models/suggestion';

import { YouTubeDataService } from './services/youtube-data-service';


@Component({
  selector: 'suggestions',
  templateUrl: './templates/suggestions.component.html',
  styleUrls: ['./styles/app.component.css'],
  providers: [ YouTubeDataService ]
})



export class SuggestionsComponent  implements OnInit {

  autoautoLoad: boolean = false;
  playlistId: string;
  playlist: Playlist;
  suggestions: VideoInfo[];
  current: PlaylistItem;
  markedItems: PlaylistItem[];


  playlistItems: Observable<PlaylistItem[]> = new Observable<PlaylistItem[]>();
  private _playlistItems = <BehaviorSubject<PlaylistItem[]>>new BehaviorSubject([]);
  private nextPageToken = <BehaviorSubject<string>>new BehaviorSubject('');

  constructor(private route: ActivatedRoute,
              private dataService: YouTubeDataService) { }

  ngOnInit(): void {
    this.playlistId = this.route.snapshot.params['playListId'];
    if (!this.playlistId) {
      return;
    }

    this.dataService.getPlaylist(this.playlistId)
      .then(playlist =>
        this.getPlaylist(playlist)
      );

    this.playlistItems = this._playlistItems.asObservable();
    this.nextPageToken
      .distinctUntilChanged()
      .subscribe((token) => {
        this.getPlaylistItems(token);
      });
  }

  private getPlaylist(playlist: Playlist): void {
    this.playlist = playlist;
    this._playlistItems.next(this.playlist.PlaylistItems);
    this.nextPageToken.next(playlist.PlaylistItemsNextPageToken);
  }

  private getPlaylistItems(token: string)  {
    if (token === null) {
      this.playlist.PlaylistItems = this._playlistItems.getValue();
      return;
      //emit event for dragular
      //$scope.$broadcast('Playlistfetched');
    }

    this.dataService.getPlaylistItems(token, this.playlistId).subscribe(data => this.loadPlaylistItems(data));
  }

  private loadPlaylistItems(data: any): void {
    this._playlistItems.next(this._playlistItems.getValue().concat(Object.assign({}, data).Response));
    this.nextPageToken.next(data.NextPageToken);
  }

  save() {
    this.dataService.savePlaylist(this.playlist);
  };
}

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
  providers: [
    YouTubeDataService,
  ],
})



export class SuggestionsComponent  implements OnInit {

  autoautoLoad: boolean = false;
  playlistId: string;
  playlist: Playlist;
  suggestions: VideoInfo[];
  current: PlaylistItem;
  markedItems: PlaylistItem[];
  searchKey: string; //todo: convert to behavior subject


  playlistItems: Observable<PlaylistItem[]> = new Observable<PlaylistItem[]>();
  private _playlistItems = <BehaviorSubject<PlaylistItem[]>>new BehaviorSubject([]);
  private nextPageToken = <BehaviorSubject<string>>new BehaviorSubject('');

  constructor(private route: ActivatedRoute,
              private dataService: YouTubeDataService) {

  }

  onItemDrop(element: any) {
    // Get the dropped data here
    //this.droppedItems.push(e.dragData);

    console.log(element);

    //event.stopPropagation();

    //let jQueryElement = jQuery(element);
    let oldPosition = parseInt(element.data("position"));

    let playListItems = this.playlist.PlaylistItems;
    let item = playListItems.filter(item => { return item.Hash === element.id; })[0];
    let newPosition = playListItems.indexOf(item);

    if (oldPosition === newPosition) {
      return;
    }

    let positionChange, startIndex, endIndex;
    if (oldPosition > newPosition) {
      startIndex = newPosition + 1;
      endIndex = oldPosition + 1;
      positionChange = 1;
    } else {
      startIndex = oldPosition;
      endIndex = newPosition;
      positionChange = -1;
    }
    playListItems[newPosition].Position = newPosition;
    for (let i = startIndex; i < endIndex; i++) {
      playListItems[i].Position += positionChange;
    }
    this._playlistItems.next(playListItems); //update list
  }

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
      this.setSuggestions();
      return;
    }

    this.dataService.getPlaylistItems(token, this.playlistId).subscribe(data => this.loadPlaylistItems(data));
  }

  private loadPlaylistItems(data: any): void {
    this._playlistItems.next(this._playlistItems.getValue().concat(Object.assign({}, data).Response));
    this.nextPageToken.next(data.NextPageToken);
  }

  private setSuggestions(): void {
    this.playlist.PlaylistItems = this._playlistItems.getValue();
    this.markedItems = this.playlist.PlaylistItems.filter(function (playListItem) { return playListItem.VideoInfo.Live === false; });
    if (this.markedItems.length === 0) {
      return;
    }

    this.current = this.markedItems[0];
    this.searchKey = this.current.VideoInfo.Title;
  }

  save() {
    this.dataService.savePlaylist(this.playlist);
  };
}

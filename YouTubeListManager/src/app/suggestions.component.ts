import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

// Observable operators
import 'rxjs/add/operator/distinctUntilChanged';
//drag n drop
import { DragulaService } from 'ng2-dragula';

import { Playlist } from './models/playlist';
import { PlaylistItem } from './models/playlist-item';
import { VideoInfo } from './models/video';
import { Suggestion } from './models/suggestion';

import { YouTubeDataService } from './services/youtube-data-service';


@Component({
    selector: 'suggestions',
    templateUrl: './templates/suggestion-bak.component.html',
    styleUrls: ['./styles/app-bak.component.css'], 
    providers: [
        YouTubeDataService,
    ],
})

export class SuggestionsComponent implements OnInit {

    autoautoLoad: boolean = false;
    playlistId: string;
    playlist: Playlist = new Playlist();
    suggestions: VideoInfo[];
    current: PlaylistItem = new PlaylistItem();
    markedItems: PlaylistItem[] = new Array<PlaylistItem>();
    searchKey: string; //todo: convert to behavior subject


    playlistItems: Observable<PlaylistItem[]> = new Observable<PlaylistItem[]>();
    private _playlistItems = <BehaviorSubject<PlaylistItem[]>>new BehaviorSubject([]);
    private nextPageToken = <BehaviorSubject<string>>new BehaviorSubject('');

    constructor(private route: ActivatedRoute,
        private dragulaService: DragulaService,
        private dataService: YouTubeDataService) {
        dragulaService.setOptions('playlist-bag', {
            removeOnSpill: true
        });
        this.current.Hash = "";
        this.playlist.PlaylistItems = new Array<PlaylistItem>();
    }

    private onDrop(args: [HTMLElement, HTMLElement]) {
        let playlistItems = this.playlist.PlaylistItems;
        let observedPlaylistItems = this._playlistItems;
        setTimeout(function () {
            for (let i = 0; i < playlistItems.length; i++) {
                playlistItems[i].Position = i;
            }
            observedPlaylistItems.next(playlistItems); //update list
        }, 100);
        
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
            .subscribe((token: any) => {
                this.getPlaylistItems(token);
            });
    }

    isSelected(playlistItem: PlaylistItem): boolean {
        return this.current != undefined && this.current.Hash === playlistItem.Hash;
    }

    private getPlaylist(playlist: Playlist): void {
        this.playlist = playlist;
        this._playlistItems.next(this.playlist.PlaylistItems);
        this.nextPageToken.next(playlist.PlaylistItemsNextPageToken);
    }

    private getPlaylistItems(token: string) {
        if (token === null) {
            this.setSuggestions();

            this.dragulaService.drop.subscribe((value: any) => {
                this.onDrop(value.slice(1));
            });
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

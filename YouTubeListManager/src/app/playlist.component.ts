import { Component, OnInit } from '@angular/core';
import { Router }            from '@angular/router';

import { Observable }         from 'rxjs/Observable';
import { BehaviorSubject }    from 'rxjs/BehaviorSubject';

// Observable operators
import 'rxjs/add/operator/distinctUntilChanged';

import { YouTubeDataService } from './services/youtube-data-service';

import { Playlist } from './models/playlist';

@Component({
  selector: 'playlist',
  templateUrl: './templates/playlists-bak.component.html',
  styleUrls: ['./styles/app.component.css'],
  providers: [ YouTubeDataService ]
})

export class PlaylistComponent implements OnInit{

  playlists: Observable<Playlist[]> = new Observable<Playlist[]>();
  private _playlists = <BehaviorSubject<Playlist[]>>new BehaviorSubject([]);
  private nextPageToken = <BehaviorSubject<string>>new BehaviorSubject('');

  constructor(private router: Router,
              private dataService: YouTubeDataService
  ) {}


  selectPlayList(id: string): void {
    let link = ['/suggestions', id];
    this.router.navigate(link);
  }

  ngOnInit(): void {
    this.playlists = this._playlists.asObservable();
    this.nextPageToken
      .distinctUntilChanged()
      .subscribe((token: any) => {
        this.getPlaylists(token);
    });
  }

  private getPlaylists(token: string)  {
    //console.log(token);
    if (token === null) {
      return;
    }

    this.dataService.getPlaylists(token).subscribe(data => this.loadPlaylists(data));
  }

  private loadPlaylists(data: any): void {
    //console.log(data.Response);
    this._playlists.next(this._playlists.getValue().concat(Object.assign({}, data).Response));

    this.nextPageToken.next(data.NextPageToken);
  }

}

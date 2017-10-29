import { Injectable }     from '@angular/core';
import { Headers, Http }  from '@angular/http';

import { Observable }     from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/toPromise';

import { Playlist }     from "../models/playlist";
import { PlaylistItem } from "../models/playlist-item";
import { Response }     from "../models/response";

@Injectable()
export class YouTubeDataService {

  private serverUrl = 'http://localhost:45921';  // URL to web api
  private headers = new Headers({'Content-Type': 'application/json', 'Access-Control-Allow-Origin': '*', 'Access-Control-Request-Method': 'GET'});

  constructor(private http: Http) {
  }

  private handleError(error: any): Promise<any> {
    console.error('An error occurred', error); // for demo purposes only
    return Promise.reject(error.message || error);
  }

  getPlaylist(id: string): Promise<Playlist> {
    const url = `${this.serverUrl}/api/playlist/get/${id}`;
    return this.http.get(url).toPromise()
      .then(response => response.json() as Playlist)
      .catch(this.handleError);
  }

  getPlaylists(requestToken?: string): Observable<Response<Playlist[]>> {
    const url = (requestToken === null || requestToken === undefined || requestToken.length === 0)
      ? `${this.serverUrl}/playlist/getall`
      : `${this.serverUrl}/playlist/getall?requestToken=${requestToken}`;
    console.log(url);
    return this.http.get(url)
      .map(response => response.json() as Response<Playlist[]>);
  }

  getPlaylistItems(requestToken: string, playlistId: string): Observable<Response<PlaylistItem[]>> {
    const url = `${this.serverUrl}/api/playlistitem/get/${playlistId}/${requestToken}`;
    console.log(url);
    return this.http.get(url)
      .map(response => response.json() as Response<PlaylistItem[]>);
  }
}

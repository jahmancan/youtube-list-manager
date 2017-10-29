import { Injectable } from '@angular/core';
import { Headers, Http } from '@angular/http';

import 'rxjs/add/operator/toPromise';
import {Playlist} from "../models/playlist";


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

  getPlaylists(): Promise<Playlist[]> {
    const url = `${this.serverUrl}/playlist/getall`;
    return this.http.get(url)
      .toPromise()
      .then(response => response.json().Response as Playlist[])
      .catch(this.handleError);
  }
}

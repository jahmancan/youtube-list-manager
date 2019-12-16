import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule }    from '@angular/http';

import { AppRoutingModule }     from './app-routing.module';
import {NgDragDropModule}       from 'ng-drag-drop';

import { DurationFormatterPipe} from './Filters/DurationFormatter';

import { AppComponent }         from './app.component';
import { PlaylistComponent }    from './playlist.component';
import { SuggestionsComponent } from './suggestions.component';

import { YouTubeDataService}    from './services/youtube-data-service';


@NgModule({
  imports:      [
    BrowserModule,
    HttpModule,
    AppRoutingModule,
    NgDragDropModule.forRoot()
  ],
  declarations: [
    DurationFormatterPipe,
    AppComponent,
    PlaylistComponent,
    SuggestionsComponent
  ],
  providers: [
    YouTubeDataService,
  ],
  bootstrap:    [ AppComponent ]
})
export class AppModule { }
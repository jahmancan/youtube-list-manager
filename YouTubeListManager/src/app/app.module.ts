import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpModule }    from '@angular/http';

import { AppRoutingModule }     from './app-routing.module';
import { DragulaModule}         from 'ng2-dragula';

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
    DragulaModule
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

import { NgModule }             from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { PlaylistComponent }      from './playlist.component';
import { SuggestionsComponent }   from './suggestions.component';

const routes: Routes = [
  { path: '', redirectTo: '/playlist', pathMatch: 'full' },
  { path: 'playlist',                   component: PlaylistComponent },
  { path: 'suggestions/:playListId',    component: SuggestionsComponent },
  { path: 'ListManager/Index',          component: PlaylistComponent }
];

@NgModule({
  imports: [ RouterModule.forRoot(routes) ],
  exports: [ RouterModule ]
})
export class AppRoutingModule {}

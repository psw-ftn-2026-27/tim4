import { Routes } from '@angular/router';
import { AdminChallenges } from './admin-challenges/admin-challenges';
import { AdminWallet } from './admin-wallet/admin-wallet';
import { MapView } from './map-view/map-view';
import { Overview } from './overview/overview';
import { TourEditing } from './tour-editing/tour-editing';
import { TourPreferences } from './tour-preferences/tour-preferences';
import { TourSearch } from './tour-search/tour-search';
import { UserWallet } from './user-wallet/user-wallet';

// Design sketches with mock data. Not linked from the header; open /design-preview by hand.
export const designPreviewRoutes: Routes = [
  { path: '', component: Overview },
  { path: 'user-wallet', component: UserWallet },
  { path: 'admin-wallet', component: AdminWallet },
  { path: 'tour-search', component: TourSearch },
  { path: 'tour-editing', component: TourEditing },
  { path: 'tour-preferences', component: TourPreferences },
  { path: 'admin-challenges', component: AdminChallenges },
  { path: 'map', component: MapView },
];

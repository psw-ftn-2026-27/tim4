import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourKeyPoints } from './tour-authoring/tour-key-points/tour-key-points';
import { MyJourneys } from './tour-browsing/my-journeys/my-journeys';
import { TourDetails } from './tour-browsing/tour-details/tour-details';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { TourMapSearch } from './tour-browsing/tour-map-search/tour-map-search';
import { ActiveTour } from './tour-executions/active-tour/active-tour';
import { Preferences } from './tour-preferences/preferences/preferences';

export const explorationRoutes: Routes = [
  { path: '', component: TourList },
  { path: 'map', component: TourMapSearch },
  { path: 'mine', component: MyTours },
  { path: 'mine/:id/key-points', component: TourKeyPoints },
  { path: 'create', component: CreateTour },
  { path: 'preferences', component: Preferences },
  { path: 'active', component: ActiveTour },
  { path: 'journeys', component: MyJourneys },
  { path: 'tours/:id', component: TourDetails },
];

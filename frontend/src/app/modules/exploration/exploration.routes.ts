import { Routes } from '@angular/router';
import { CreateTour } from './tour-authoring/create-tour/create-tour';
import { MyTours } from './tour-authoring/my-tours/my-tours';
import { TourEditor } from './tour-authoring/tour-editor/tour-editor';
import { TourKeyPoints } from './tour-authoring/tour-key-points/tour-key-points';
import { TourPublish } from './tour-authoring/tour-publish/tour-publish';
import { TourTimes } from './tour-authoring/tour-times/tour-times';
import { TourDetails } from './tour-browsing/tour-details/tour-details';
import { TourList } from './tour-browsing/tour-list/tour-list';
import { ActiveTour } from './tour-executions/active-tour/active-tour';
import { MyJourneys } from './tour-executions/my-journeys/my-journeys';
import { Preferences } from './tour-preferences/preferences/preferences';
import { requireRole } from '../../core/auth/require-role';

export const explorationRoutes: Routes = [
  { path: '', component: TourList, canActivate: [requireRole()] },
  { path: 'mine', component: MyTours, canActivate: [requireRole('explorer')] },
  { path: 'mine/:id', component: TourEditor, canActivate: [requireRole('explorer')] },
  { path: 'mine/:id/key-points', component: TourKeyPoints, canActivate: [requireRole('explorer')] },
  { path: 'mine/:id/times', component: TourTimes, canActivate: [requireRole('explorer')] },
  { path: 'mine/:id/publish', component: TourPublish, canActivate: [requireRole('explorer')] },
  { path: 'create', component: CreateTour, canActivate: [requireRole('explorer')] },
  { path: 'preferences', component: Preferences, canActivate: [requireRole('explorer')] },
  { path: 'journeys', component: MyJourneys, canActivate: [requireRole('explorer')] },
  { path: 'active', component: ActiveTour, canActivate: [requireRole('explorer')] },
  { path: 'tours/:id', component: TourDetails, canActivate: [requireRole()] },
];

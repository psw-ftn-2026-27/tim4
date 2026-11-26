import { Routes } from '@angular/router';
import { LocationChallengeCreate } from './location-challenges/location-challenge-create/location-challenge-create';
import { LocationChallengeDetails } from './location-challenges/location-challenge-details/location-challenge-details';
import { LocationChallengeList } from './location-challenges/location-challenge-list/location-challenge-list';
import { LocationChallengePlay } from './location-challenges/location-challenge-play/location-challenge-play';
import { ChallengeList } from './quiz-authoring/challenge-list/challenge-list';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { QuizDetail } from './quiz-authoring/quiz-detail/quiz-detail';
import { QuizMap } from './quiz-solving/quiz-map/quiz-map';
import { QuizSolve } from './quiz-solving/quiz-solve/quiz-solve';
import { SocialChallengeList } from './social-challenge-preparation/social-challenge-list/social-challenge-list';
import { SocialChallengeEditor } from './social-challenge-preparation/social-challenge-editor/social-challenge-editor';
import { requireRole } from '../../core/auth/require-role';

export const gamesRoutes: Routes = [
  { path: '', component: QuizMap, canActivate: [requireRole('explorer')] },
  { path: 'admin', component: ChallengeList, canActivate: [requireRole('administrator')] },
  // The old address of the challenges map, kept so saved links still work.
  { path: 'map', redirectTo: '', pathMatch: 'full' },
  { path: 'quizzes/create', component: CreateQuiz, canActivate: [requireRole('administrator')] },
  { path: 'quizzes/:id/solve', component: QuizSolve, canActivate: [requireRole('explorer')] },
  { path: 'quizzes/:id', component: QuizDetail, canActivate: [requireRole('administrator')] },
  {
    path: 'admin/social',
    component: SocialChallengeList,
    canActivate: [requireRole('administrator')],
  },
  {
    path: 'social-challenges/create',
    component: SocialChallengeEditor,
    canActivate: [requireRole('administrator')],
  },
  {
    path: 'social-challenges/:id/edit',
    component: SocialChallengeEditor,
    canActivate: [requireRole('administrator')],
  },
  {
    path: 'admin/location',
    component: LocationChallengeList,
    canActivate: [requireRole('administrator')],
  },
  {
    path: 'location-challenges/create',
    component: LocationChallengeCreate,
    canActivate: [requireRole('administrator')],
  },
  {
    path: 'location-challenges/:id/play',
    component: LocationChallengePlay,
    canActivate: [requireRole('explorer')],
  },
  {
    path: 'location-challenges/:id',
    component: LocationChallengeDetails,
    canActivate: [requireRole('administrator')],
  },
];

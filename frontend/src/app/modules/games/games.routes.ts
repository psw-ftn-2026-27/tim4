import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
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
  { path: '', component: GamesHome, canActivate: [requireRole()] },
  { path: 'challenges', component: ChallengeList, canActivate: [requireRole('administrator')] },
  { path: 'map', component: QuizMap, canActivate: [requireRole('explorer')] },
  { path: 'quizzes/create', component: CreateQuiz, canActivate: [requireRole('administrator')] },
  { path: 'quizzes/:id/solve', component: QuizSolve, canActivate: [requireRole('explorer')] },
  { path: 'quizzes/:id', component: QuizDetail, canActivate: [requireRole('administrator')] },
  {
    path: 'social-challenges',
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
  { path: 'location-challenges', component: LocationChallengeList, canActivate: [requireRole()] },
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

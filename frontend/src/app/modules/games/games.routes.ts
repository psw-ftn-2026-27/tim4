import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { ChallengeList } from './quiz-authoring/challenge-list/challenge-list';
import { CreateQuiz } from './quiz-authoring/create-quiz/create-quiz';
import { QuizDetail } from './quiz-authoring/quiz-detail/quiz-detail';
import { SocialChallengeList } from './social-challenge-preparation/social-challenge-list/social-challenge-list';
import { SocialChallengeEditor } from './social-challenge-preparation/social-challenge-editor/social-challenge-editor';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'challenges', component: ChallengeList },
  { path: 'quizzes/create', component: CreateQuiz },
  { path: 'quizzes/:id', component: QuizDetail },
  { path: 'social-challenges', component: SocialChallengeList },
  { path: 'social-challenges/create', component: SocialChallengeEditor },
  { path: 'social-challenges/:id/edit', component: SocialChallengeEditor },
];

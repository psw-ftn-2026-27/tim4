import { Routes } from '@angular/router';
import { GamesHome } from './games-home/games-home';
import { SocialChallengeList } from './social-challenge-preparation/social-challenge-list/social-challenge-list';
import { SocialChallengeEditor } from './social-challenge-preparation/social-challenge-editor/social-challenge-editor';

export const gamesRoutes: Routes = [
  { path: '', component: GamesHome },
  { path: 'social-challenges', component: SocialChallengeList },
  { path: 'social-challenges/create', component: SocialChallengeEditor },
  { path: 'social-challenges/:id/edit', component: SocialChallengeEditor },
];

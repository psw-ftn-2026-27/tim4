import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

type ChallengeStatus = 'Draft' | 'Published' | 'Retired' | 'Archived';

interface AdminChallenge {
  id: number;
  name: string;
  kind: 'Društveni izazov' | 'Kviz';
  status: ChallengeStatus;
  xp: number;
}

interface QuizQuestion {
  id: number;
  text: string;
  answers: string[];
  correctIndex: number;
}

// Mock data: every state appears at least once, for both kinds of challenge.
const CHALLENGES: AdminChallenge[] = [
  { id: 1, name: 'Zalazak na Petrovaradinu', kind: 'Društveni izazov', status: 'Draft', xp: 50 },
  { id: 2, name: 'Istorija Novog Sada', kind: 'Kviz', status: 'Published', xp: 80 },
  { id: 3, name: 'Jutarnji Štrand', kind: 'Društveni izazov', status: 'Retired', xp: 40 },
  { id: 4, name: 'Mostovi na Dunavu', kind: 'Kviz', status: 'Archived', xp: 60 },
  { id: 5, name: 'Tvrđava noću', kind: 'Kviz', status: 'Draft', xp: 70 },
];

const QUESTIONS: QuizQuestion[] = [
  {
    id: 1,
    text: 'Koje godine je završena izgradnja Petrovaradinske tvrđave?',
    answers: ['1692.', '1780.', '1848.'],
    correctIndex: 1,
  },
  {
    id: 2,
    text: 'Koja reka protiče kroz Novi Sad?',
    answers: ['Sava', 'Tisa', 'Dunav'],
    correctIndex: 2,
  },
];

@Component({
  imports: [RouterLink],
  selector: 'app-admin-challenges',
  styleUrl: './admin-challenges.scss',
  templateUrl: './admin-challenges.html',
})
export class AdminChallenges {
  protected readonly challenges = CHALLENGES;
  protected readonly questions = QUESTIONS;

  // Editing is allowed only in Draft.
  protected canEdit(challenge: AdminChallenge): boolean {
    return challenge.status === 'Draft';
  }

  // Deleting is allowed only in Draft and Archived.
  protected canDelete(challenge: AdminChallenge): boolean {
    return challenge.status === 'Draft' || challenge.status === 'Archived';
  }
}

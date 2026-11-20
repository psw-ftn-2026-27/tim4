import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AnswerDto, QuizPositionDto, SocialChallengePositionDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';
const SOCIAL_CHALLENGES_URL = '/api/games/social-challenges';

// The commands of the challenge map: quizzes and, on the same page, social challenges.
@Injectable({ providedIn: 'root' })
export class QuizSolving {
  private readonly http = inject(HttpClient);

  async start(quizId: string, position: QuizPositionDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${quizId}/attempt`, position));
  }

  async answer(quizId: string, questionId: string, answer: AnswerDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${BASE_URL}/${quizId}/attempt/questions/${questionId}/answer`, answer),
    );
  }

  async activate(challengeId: string, position: SocialChallengePositionDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${challengeId}/run`, position),
    );
  }

  async reportLocation(challengeId: string, position: SocialChallengePositionDto): Promise<void> {
    await firstValueFrom(
      this.http.post<void>(`${SOCIAL_CHALLENGES_URL}/${challengeId}/run/location`, position),
    );
  }
}

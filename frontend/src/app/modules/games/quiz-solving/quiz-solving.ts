import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AnswerDto, QuizPositionDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

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
}

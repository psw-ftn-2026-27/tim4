import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { QuizContentDto } from '../api/games-api-types';

const BASE_URL = '/api/games/quizzes';

@Injectable({ providedIn: 'root' })
export class QuizAuthoring {
  private readonly http = inject(HttpClient);

  async create(dto: QuizContentDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }

  async edit(id: string, dto: QuizContentDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/${id}`, dto));
  }
}

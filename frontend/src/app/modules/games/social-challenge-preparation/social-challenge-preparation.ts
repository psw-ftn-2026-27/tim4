import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { SocialChallengeDetailsDto } from '../api/games-api-types';

const baseUrl = '/api/games/social-challenges';

@Injectable({ providedIn: 'root' })
export class SocialChallengePreparation {
  private readonly http = inject(HttpClient);

  create(details: SocialChallengeDetailsDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(baseUrl, details));
  }

  async update(id: string, details: SocialChallengeDetailsDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${baseUrl}/${id}`, details));
  }

  async remove(id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${baseUrl}/${id}`));
  }
}

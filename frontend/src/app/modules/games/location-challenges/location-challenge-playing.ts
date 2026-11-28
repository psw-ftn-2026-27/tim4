import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CheckLocationDto } from '../api/games-api-types';

@Injectable({ providedIn: 'root' })
export class LocationChallengePlaying {
  private readonly http = inject(HttpClient);

  activate(id: string, position: CheckLocationDto): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`/api/games/location-challenges/${id}/attempt`, position),
    );
  }

  check(id: string, pointId: string, position: CheckLocationDto): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(
        `/api/games/location-challenges/${id}/attempt/points/${pointId}/check`,
        position,
      ),
    );
  }
}

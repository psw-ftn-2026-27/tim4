import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TourPreferenceDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/preferences';

@Injectable({ providedIn: 'root' })
export class TourPreferences {
  private readonly http = inject(HttpClient);

  async save(dto: TourPreferenceDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(BASE_URL, dto));
  }

  async remove(): Promise<void> {
    await firstValueFrom(this.http.delete<void>(BASE_URL));
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CoordinatesDto } from '../api/coordinates-dto';

const BASE_URL = '/api/positions/mine';

// The one command of the position simulator: it saves the signed-in tourist's position.
// Pages read the position themselves, with an httpResource of /api/positions/mine.
@Injectable({ providedIn: 'root' })
export class PositionSimulation {
  private readonly http = inject(HttpClient);

  async setPosition(dto: CoordinatesDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(BASE_URL, dto));
  }
}

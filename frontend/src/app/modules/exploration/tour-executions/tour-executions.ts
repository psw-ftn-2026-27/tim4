import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CheckPositionDto, PositionCheckResultDto } from '../api/exploration-api-types';

const ACTIVE_URL = '/api/exploration/tour-executions/active';

@Injectable({ providedIn: 'root' })
export class TourExecutions {
  private readonly http = inject(HttpClient);

  // The server decides whether the tourist reached the next key point and returns only the
  // outcome; the page reads the rest through its queries.
  async checkPosition(dto: CheckPositionDto): Promise<PositionCheckResultDto> {
    return await firstValueFrom(
      this.http.post<PositionCheckResultDto>(`${ACTIVE_URL}/position-checks`, dto),
    );
  }
}

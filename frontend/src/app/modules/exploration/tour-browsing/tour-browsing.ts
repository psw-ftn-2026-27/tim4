import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BarRefresh } from '../../../shared/bar/bar-refresh';
import { RecordLocationSearchDto, StartTourDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/location-searches';
const TOUR_EXECUTIONS_URL = '/api/exploration/tour-executions';

@Injectable({ providedIn: 'root' })
export class TourBrowsing {
  private readonly http = inject(HttpClient);
  private readonly barRefresh = inject(BarRefresh);

  async recordLocationSearch(dto: RecordLocationSearchDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(BASE_URL, dto));
  }

  // Payment checks the cart rules; a rejection arrives as its ProblemDetails.
  async addToCart(tourId: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`/api/exploration/tours/${tourId}/cart`, {}));
    // The cart count in the bar changes while the tourist stays on the tour's page.
    this.barRefresh.refresh();
  }

  // The tour details page starts the tour, so the command lives in this group's service even
  // though the server serves it from the TourExecutions group.
  async startTour(dto: StartTourDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(TOUR_EXECUTIONS_URL, dto));
  }
}

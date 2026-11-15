import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Analytics } from '../../../shared/analytics/analytics';
import { CreateTourDto, PublishTourDto, TourDto, TransportTimeDto } from '../api/exploration-api-types';

const BASE_URL = '/api/exploration/tours';

@Injectable({ providedIn: 'root' })
export class TourAuthoring {
  private readonly http = inject(HttpClient);
  private readonly analytics = inject(Analytics);

  async create(dto: CreateTourDto): Promise<TourDto> {
    const tour = await firstValueFrom(this.http.post<TourDto>(BASE_URL, dto));
    this.analytics.track('tour_created', {});
    return tour;
  }

  async publish(id: string, dto: PublishTourDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/publish`, dto));
    this.analytics.track('tour_published', {});
  }

  async archive(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/archive`, {}));
  }

  async reactivate(id: string): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/reactivate`, {}));
  }

  async addTransportTime(id: string, dto: TransportTimeDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${id}/transport-times`, dto));
  }
}

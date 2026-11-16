import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { Analytics } from '../../../shared/analytics/analytics';
import {
  CreateTourDto,
  KeyPointDetailsDto,
  PublishTourDto,
  TourDto,
  TransportTimeDto,
} from '../api/exploration-api-types';

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

  async addKeyPoint(tourId: string, dto: KeyPointDetailsDto, image: File): Promise<void> {
    const body = keyPointForm(dto, image);
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/${tourId}/key-points`, body));
  }

  // Without a new image the key point keeps the image it already has.
  async updateKeyPoint(
    tourId: string,
    keyPointId: string,
    dto: KeyPointDetailsDto,
    image: File | null,
  ): Promise<void> {
    const body = keyPointForm(dto, image);
    await firstValueFrom(
      this.http.put<void>(`${BASE_URL}/${tourId}/key-points/${keyPointId}`, body),
    );
  }

  async removeKeyPoint(tourId: string, keyPointId: string, tourLengthInKm: number): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(`${BASE_URL}/${tourId}/key-points/${keyPointId}`, {
        params: { tourLengthInKm },
      }),
    );
  }
}

// The fields and the image go together in one request, as the server expects a form.
function keyPointForm(dto: KeyPointDetailsDto, image: File | null): FormData {
  const body = new FormData();
  body.append('name', dto.name);
  body.append('description', dto.description);
  if (dto.secret !== null) {
    body.append('secret', dto.secret);
  }
  body.append('latitude', String(dto.latitude));
  body.append('longitude', String(dto.longitude));
  body.append('isVisibleWithoutPurchase', String(dto.isVisibleWithoutPurchase));
  body.append('tourLengthInKm', String(dto.tourLengthInKm));
  if (image !== null) {
    body.append('image', image);
  }
  return body;
}

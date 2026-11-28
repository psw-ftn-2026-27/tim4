import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { LocationChallengeContentDto } from '../api/games-api-types';

@Injectable({ providedIn: 'root' })
export class LocationChallengeAuthoring {
  private readonly http = inject(HttpClient);

  create(content: LocationChallengeContentDto): Promise<string> {
    const request = new FormData();
    request.append('name', content.name);
    request.append('startLatitude', String(content.startLatitude));
    request.append('startLongitude', String(content.startLongitude));
    request.append('xpReward', String(content.xpReward));
    content.points.forEach((point, index) => {
      request.append(`points[${index}].latitude`, String(point.latitude));
      request.append(`points[${index}].longitude`, String(point.longitude));
      request.append(`points[${index}].image`, point.image);
    });
    return firstValueFrom(this.http.post<string>('/api/games/location-challenges', request));
  }
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { LocationMap } from '../../../../shared/map/location-map';
import { LocationChallengeDetailsDto } from '../../api/games-api-types';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-location-challenge-details',
  templateUrl: './location-challenge-details.html',
})
export class LocationChallengeDetails {
  readonly id = input.required<string>();
  private readonly auth = inject(Auth);
  protected readonly user = this.auth.user;
  protected readonly isAdministrator = computed(
    () => this.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly details = httpResource<LocationChallengeDetailsDto>(() =>
    this.isAdministrator() ? `/api/games/location-challenges/${this.id()}/admin` : undefined,
  );
  protected readonly locations = computed(() => {
    const details = this.details.value();
    if (!details) return [];
    return [
      {
        latitude: details.challenge.startLatitude,
        longitude: details.challenge.startLongitude,
        name: 'Početak',
      },
      ...details.points.map((point, index) => ({
        latitude: point.latitude,
        longitude: point.longitude,
        name: `Skrivena tačka ${index + 1}`,
      })),
    ];
  });
}

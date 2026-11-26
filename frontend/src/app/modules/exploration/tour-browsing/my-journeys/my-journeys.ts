import { httpResource } from '@angular/common/http';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';
import { PurchasedTourDto } from '../../api/exploration-api-types';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-my-journeys',
  styleUrl: './my-journeys.scss',
  templateUrl: './my-journeys.html',
})
export class MyJourneys {
  protected readonly auth = inject(Auth);

  // Only a tourist buys tours, so the request is made only for one.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // Every tour the tourist bought, free ones included, archived ones too. Without purchases the
  // server sends an empty list, not an error.
  protected readonly tours = httpResource<PurchasedTourDto[]>(() =>
    this.isExplorer() ? '/api/exploration/tours/purchased' : undefined,
  );

  // One pin per tour, at the place where the tour starts.
  protected readonly pins = computed<MapLocation[]>(() =>
    (this.tours.value() ?? []).map((item) => ({
      latitude: item.pinLocation.latitude,
      longitude: item.pinLocation.longitude,
      name: item.tour.name,
    })),
  );

  // The length in kilometres with one decimal and a decimal comma, for example "3,2".
  protected length(lengthInKm: number): string {
    return lengthInKm.toFixed(1).replace('.', ',');
  }
}

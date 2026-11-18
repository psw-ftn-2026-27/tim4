import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';
import { TourDetailsDto } from '../../api/exploration-api-types';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  // The tour ID from the route tours/:id.
  readonly id = input.required<string>();

  // The server sends only what this visitor may see: a visitor gets the key points open
  // without purchase and no secrets, the author gets the whole tour.
  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly mapLocations = computed<MapLocation[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((keyPoint) => ({
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
    })),
  );
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';
import { ActiveTourDto } from '../../api/exploration-api-types';

// The active tour screen. It asks the server for the tourist's tour in progress, so it shows
// the same after a refresh or a return.
@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-active-tour',
  styleUrl: './active-tour.scss',
  templateUrl: './active-tour.html',
})
export class ActiveTour {
  protected readonly auth = inject(Auth);

  // Only a tourist has a tour in progress and a position, so the requests are made only for one.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // The server answers 204 No Content when no tour is in progress, so the value is null.
  protected readonly activeTour = httpResource<ActiveTourDto | null>(() =>
    this.isExplorer() ? '/api/exploration/tour-executions/active' : undefined,
  );

  // The server answers 204 No Content when the tourist has no position, so the value is null.
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );

  protected readonly savedPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );

  protected readonly mapLocations = computed<MapLocation[]>(() => {
    const active = this.activeTour.hasValue() ? this.activeTour.value() : null;
    return (active?.keyPoints ?? []).map((keyPoint) => ({
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
    }));
  });

  // The next key point is the map's target, so the map draws the route from the position to it.
  protected readonly nextKeyPoint = computed(() => {
    const active = this.activeTour.hasValue() ? this.activeTour.value() : null;
    return active?.keyPoints.find((keyPoint) => keyPoint.id === active.nextKeyPointId) ?? null;
  });

  protected reload(): void {
    this.activeTour.reload();
    this.position.reload();
  }
}

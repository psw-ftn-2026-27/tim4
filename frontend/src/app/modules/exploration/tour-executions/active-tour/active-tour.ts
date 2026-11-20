import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { serverMessage } from '../../../../shared/util/server-message';
import { ActiveTourDto, TourExecutionDto } from '../../api/exploration-api-types';
import { TourExecutions } from '../tour-executions';

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
  private readonly router = inject(Router);
  private readonly positionSimulation = inject(PositionSimulation);
  private readonly tourExecutions = inject(TourExecutions);

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

  // Set once a check completes the tour. The session is no longer in progress, so the page
  // reads it by its ID instead of through the active tour.
  protected readonly completedExecutionId = signal<string | null>(null);

  protected readonly completedExecution = httpResource<TourExecutionDto>(() => {
    const id = this.completedExecutionId();
    return id === null ? undefined : `/api/exploration/tour-executions/${id}`;
  });

  protected readonly pending = signal(false);
  protected readonly positionError = signal<string | null>(null);
  protected readonly checkError = signal<string | null>(null);
  protected readonly abandonError = signal<string | null>(null);

  // True while the section that confirms abandoning the tour is open.
  protected readonly confirmingAbandon = signal(false);

  // True once a check reached a key point; the notice stays until another one is reached.
  private readonly justReached = signal(false);

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

  // The newly reached key point is the last one in the reloaded active tour, so the notice
  // waits until the reload has finished.
  protected readonly reachedKeyPoint = computed(() => {
    if (!this.justReached() || this.activeTour.status() !== 'resolved') {
      return null;
    }
    return this.activeTour.value()?.reachedKeyPoints.at(-1) ?? null;
  });

  protected reload(): void {
    this.activeTour.reload();
    this.position.reload();
  }

  // A click on the map saves the position; the marker moves only once the resource reloads.
  protected async setPosition(coordinates: MapCoordinates): Promise<void> {
    const active = this.activeTour.hasValue() ? this.activeTour.value() : null;
    if (active === null || this.pending()) {
      return;
    }
    this.pending.set(true);
    this.positionError.set(null);
    this.checkError.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
      // Until the platform team provides the pattern for the 10-second timer, the page checks
      // the position right after it is saved. The timer is added once the pattern exists.
      await this.checkPosition(coordinates, active.executionId);
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // After abandoning, the tourist returns to the details of the abandoned tour, where it can be
  // started again as a new session.
  protected async abandon(tourId: string): Promise<void> {
    if (this.pending()) {
      return;
    }
    this.pending.set(true);
    this.abandonError.set(null);
    try {
      await this.tourExecutions.abandon();
      await this.router.navigate(['/exploration/tours', tourId]);
    } catch (failure) {
      this.abandonError.set(serverMessage(failure, 'Turu nije moguće napustiti.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Sends the saved coordinates; the server decides whether the next key point is reached.
  private async checkPosition(coordinates: MapCoordinates, executionId: string): Promise<void> {
    try {
      const result = await this.tourExecutions.checkPosition({
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
      });
      if (result.outcome === 'ReachedKeyPoint') {
        this.justReached.set(true);
        this.activeTour.reload();
      } else if (result.outcome === 'CompletedTour') {
        this.completedExecutionId.set(executionId);
      }
    } catch (failure) {
      this.checkError.set(serverMessage(failure, 'Provera pozicije nije uspela.'));
    }
  }
}

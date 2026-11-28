import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapArea } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { formatDateTime } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengePlayDto } from '../../api/games-api-types';
import { LocationChallengePlaying } from '../location-challenge-playing';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-location-challenge-play',
  templateUrl: './location-challenge-play.html',
  styleUrl: './location-challenge-play.scss',
})
export class LocationChallengePlay {
  readonly id = input.required<string>();
  private readonly auth = inject(Auth);
  private readonly playing = inject(LocationChallengePlaying);
  private readonly simulation = inject(PositionSimulation);

  protected readonly user = this.auth.user;
  protected readonly isExplorer = computed(() => this.user()?.roles.includes('explorer') ?? false);
  protected readonly details = httpResource<LocationChallengePlayDto>(() =>
    this.isExplorer() ? `/api/games/location-challenges/${this.id()}/attempt` : undefined,
  );
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );
  protected readonly detailsMissing = computed(() => {
    const error = this.details.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });
  protected readonly pending = signal(false);
  protected readonly feedback = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = computed(
    () => this.pending() || this.details.isLoading() || this.position.isLoading(),
  );
  protected readonly canCheck = computed(
    () => !this.busy() && this.position.hasValue() && this.position.value() !== null,
  );
  protected readonly locations = computed(() => {
    if (!this.details.hasValue()) return [];
    const { name, startLatitude, startLongitude } = this.details.value().challenge;
    return [
      {
        name,
        latitude: startLatitude,
        longitude: startLongitude,
        label: '⌖ Start',
        tone: 'location' as const,
      },
    ];
  });
  protected readonly area = computed<MapArea | null>(() => {
    if (!this.details.hasValue()) return null;
    const { attempt, challenge } = this.details.value();
    return attempt && !attempt.completedAt
      ? {
          latitude: challenge.startLatitude,
          longitude: challenge.startLongitude,
          radiusMeters: attempt.radiusInMeters,
        }
      : null;
  });
  protected readonly formatDateTime = formatDateTime;

  protected readonly savedPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );

  protected readonly discoveredCount = computed(() =>
    this.details.hasValue()
      ? (this.details.value().attempt?.points.filter((point) => point.discovered).length ?? 0)
      : 0,
  );

  protected async setPosition(coordinates: CoordinatesDto): Promise<void> {
    if (this.pending()) return;
    await this.perform(async () => {
      await this.simulation.setPosition(coordinates);
      this.position.reload();
    }, 'Poziciju nije moguće sačuvati.');
  }

  protected async activate(): Promise<void> {
    if (!this.canCheck() || !this.position.hasValue()) return;
    const coordinates = this.position.value();
    if (!coordinates) return;
    await this.perform(async () => {
      await this.playing.activate(this.id(), coordinates);
      this.details.reload();
    }, 'Aktivacija izazova nije uspela.');
  }

  protected async check(pointId: string): Promise<void> {
    if (!this.canCheck() || !this.position.hasValue()) return;
    const coordinates = this.position.value();
    if (!coordinates) return;
    await this.perform(async () => {
      await this.playing.check(this.id(), pointId, coordinates);
      this.feedback.set('Skrivena tačka je otkrivena.');
      this.details.reload();
    }, 'Provera lokacije nije uspela.');
  }

  private async perform(action: () => Promise<void>, fallback: string): Promise<void> {
    this.error.set(null);
    this.feedback.set(null);
    this.pending.set(true);
    try {
      await action();
    } catch (failure) {
      if (failure instanceof HttpErrorResponse && failure.status === 409) {
        this.details.reload();
      }
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.pending.set(false);
    }
  }
}

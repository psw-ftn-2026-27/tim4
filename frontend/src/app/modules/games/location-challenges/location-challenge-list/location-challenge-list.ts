import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationMap } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeDto } from '../../api/games-api-types';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-location-challenge-list',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  private readonly auth = inject(Auth);
  private readonly positionSimulation = inject(PositionSimulation);
  protected readonly user = this.auth.user;
  protected readonly isAdministrator = computed(
    () => this.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly canBrowse = computed(
    () => this.isAdministrator() || (this.user()?.roles.includes('explorer') ?? false),
  );
  protected readonly canSetPosition = computed(
    () => this.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.canSetPosition() ? '/api/positions/mine' : undefined,
  );
  protected readonly positionPending = signal(false);
  protected readonly positionError = signal<string | null>(null);
  protected readonly page = signal(1);
  protected readonly pageSize = 10;
  protected readonly pageCount = computed(() =>
    Math.ceil((this.challenges.value()?.totalCount ?? 0) / this.pageSize),
  );
  protected readonly challenges = httpResource<PageResult<LocationChallengeDto>>(() =>
    this.canBrowse()
      ? `/api/games/location-challenges?page=${this.page()}&pageSize=${this.pageSize}`
      : undefined,
  );
  protected readonly locations = computed(
    () =>
      this.challenges.value()?.items.map((challenge) => ({
        latitude: challenge.startLatitude,
        longitude: challenge.startLongitude,
        name: challenge.name,
      })) ?? [],
  );

  protected async setPosition(coordinates: CoordinatesDto): Promise<void> {
    if (this.positionPending()) return;
    this.positionPending.set(true);
    this.positionError.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.positionPending.set(false);
    }
  }
}

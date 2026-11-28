import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { serverMessage } from '../../../../shared/util/server-message';
import { formatKm, formatMinutes } from '../../../../shared/util/format';
import { TourDetailsDto } from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';
import { difficultyBadges, difficultyLabels, transportLabels } from '../tour-card/tour-card';

@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-tour-details',
  styleUrl: './tour-details.scss',
  templateUrl: './tour-details.html',
})
export class TourDetails {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly positionSimulation = inject(PositionSimulation);
  private readonly tourBrowsing = inject(TourBrowsing);

  // The tour ID from the route tours/:id.
  readonly id = input.required<string>();

  // The server sends only what this visitor may see: the author gets the whole tour, a buyer
  // every key point without secrets, anyone else the key points open without purchase and no
  // secrets.
  protected readonly tour = httpResource<TourDetailsDto>(
    () => `/api/exploration/tours/${this.id()}`,
  );

  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyBadges = difficultyBadges;

  // Length and the time for each way of travel, in one row of facts under the name.
  protected readonly facts = computed(() => {
    const tour = this.tour.value();
    if (tour === undefined) {
      return [];
    }
    return [
      formatKm(tour.lengthInKm),
      ...tour.transportTimes.map(
        (time) => `${transportLabels[time.transport]} ${formatMinutes(time.minutes)}`,
      ),
    ];
  });

  protected readonly isAuthor = computed(
    () => this.tour.value()?.authorId === this.auth.user()?.id,
  );

  // The key point under the mouse, in the list or on the map, highlighted in both.
  protected readonly hovered = signal<number | null>(null);

  // The author and a buyer get every key point, so the path between them is the real tour.
  // Anyone else gets only the open ones, and a path joining them would skip the hidden ones.
  protected readonly seesWholeTour = computed(() => {
    const tour = this.tour.value();
    if (tour === undefined) {
      return false;
    }
    return tour.isPurchased || tour.authorId === this.auth.user()?.id;
  });

  // Shown to a visitor who has not bought the tour and to its author, never to a buyer.
  protected readonly priceText = computed(() => {
    const price = this.tour.value()?.price ?? null;
    if (price === null) {
      return 'Cena još nije određena';
    }
    return price === 0 ? 'Besplatno' : `${price} EC`;
  });

  protected readonly pending = signal(false);
  protected readonly added = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly positionError = signal<string | null>(null);
  protected readonly startError = signal<string | null>(null);

  // Only a signed-in explorer who is not the author and has not bought the tour may add it.
  protected readonly canAddToCart = computed(() => {
    const user = this.auth.user();
    const tour = this.tour.value();
    if (user === null || tour === undefined) {
      return false;
    }
    return user.roles.includes('explorer') && user.id !== tour.authorId && !tour.isPurchased;
  });

  // Only a tourist has a position, so the request is made only for one.
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // The server answers 204 No Content when the tourist has no position, so the value is null.
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );

  // The last position saved on the server; null while it is not set, loading or not loaded.
  protected readonly savedPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );

  // Numbered pins, in the order the tourist walks them.
  protected readonly mapLocations = computed<MapLocation[]>(() =>
    (this.tour.value()?.keyPoints ?? []).map((keyPoint, index) => ({
      latitude: keyPoint.latitude,
      longitude: keyPoint.longitude,
      name: keyPoint.name,
      label: String(index + 1),
    })),
  );

  // The tourist stays on the page; the cart is Payment's screen, reached only by a link.
  protected async addToCart(): Promise<void> {
    this.pending.set(true);
    this.added.set(false);
    this.error.set(null);
    try {
      await this.tourBrowsing.addToCart(this.id());
      this.added.set(true);
      this.tour.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Dodavanje u korpu nije uspelo. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  // A click on the map saves the position; the marker moves only once the resource reloads.
  protected async setPosition(coordinates: MapCoordinates): Promise<void> {
    if (this.pending()) {
      return;
    }
    this.pending.set(true);
    this.positionError.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // Sends the last saved position with the command; without one the button is disabled.
  protected async startTour(tourId: string): Promise<void> {
    const position = this.savedPosition();
    if (position === null || this.pending()) {
      return;
    }
    this.pending.set(true);
    this.startError.set(null);
    try {
      await this.tourBrowsing.startTour({
        tourId,
        latitude: position.latitude,
        longitude: position.longitude,
      });
      await this.router.navigate(['/exploration/active']);
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'Turu nije moguće pokrenuti.'));
    } finally {
      this.pending.set(false);
    }
  }
}

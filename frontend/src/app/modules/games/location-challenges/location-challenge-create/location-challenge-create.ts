import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, maxLength, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { LocationMap, MapCoordinates } from '../../../../shared/map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { LocationChallengeAuthoring } from '../location-challenge-authoring';

interface PointEntry {
  key: number;
  location: MapCoordinates | null;
  image: File | null;
}

@Component({
  imports: [FormField, ImageUpload, LocationMap, RouterLink],
  selector: 'app-location-challenge-create',
  templateUrl: './location-challenge-create.html',
})
export class LocationChallengeCreate {
  private readonly auth = inject(Auth);
  private readonly authoring = inject(LocationChallengeAuthoring);
  private readonly router = inject(Router);
  private nextPointKey = 1;

  protected readonly user = this.auth.user;
  protected readonly isAdministrator = computed(
    () => this.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly challengeForm = form(
    signal({ name: '', start: null as MapCoordinates | null, xpReward: null as number | null }),
    (path) => {
      required(path.name, { message: 'Unesite naziv izazova.' });
      maxLength(path.name, 200, { message: 'Naziv može imati najviše 200 znakova.' });
      required(path.start, { message: 'Izaberite početnu lokaciju.' });
      required(path.xpReward, { message: 'Unesite XP nagradu.' });
      min(path.xpReward, 1, { message: 'XP nagrada mora biti veća od nule.' });
    },
  );
  protected readonly points = signal<PointEntry[]>([{ key: 0, location: null, image: null }]);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly submitted = signal(false);

  protected addPoint(): void {
    this.points.update((points) => [
      ...points,
      { key: this.nextPointKey++, location: null, image: null },
    ]);
  }

  protected removePoint(key: number): void {
    this.points.update((points) => points.filter((point) => point.key !== key));
  }

  protected setLocation(key: number, location: MapCoordinates): void {
    this.points.update((points) =>
      points.map((point) => (point.key === key ? { ...point, location } : point)),
    );
  }

  protected setImage(key: number, image: File | null): void {
    this.points.update((points) =>
      points.map((point) => (point.key === key ? { ...point, image } : point)),
    );
  }

  protected async save(): Promise<void> {
    if (this.pending()) return;
    this.error.set(null);
    this.submitted.set(true);
    this.challengeForm().markAsTouched();
    const { name, start, xpReward } = this.challengeForm().value();
    if (this.challengeForm().invalid() || !start || xpReward === null) return;
    if (!Number.isInteger(xpReward)) {
      this.error.set('Unesite ceo broj za XP nagradu.');
      return;
    }

    const points = [];
    for (const point of this.points()) {
      if (!point.location || !point.image) return;
      points.push({ ...point.location, image: point.image });
    }

    this.pending.set(true);
    try {
      const id = await this.authoring.create({
        name,
        startLatitude: start.latitude,
        startLongitude: start.longitude,
        xpReward,
        points,
      });
      await this.router.navigate(['/games/location-challenges', id]);
    } catch (failure) {
      if (failure instanceof HttpErrorResponse && failure.status === 413) {
        this.error.set('Ukupna veličina zahteva prelazi 6 MiB. Izaberite manje fotografije.');
      } else {
        this.error.set(serverMessage(failure, 'Čuvanje lokacijskog izazova nije uspelo.'));
      }
    } finally {
      this.pending.set(false);
    }
  }
}

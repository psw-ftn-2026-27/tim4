import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeyPointDto, TourDto } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

const EMPTY_KEY_POINT = {
  name: '',
  description: '',
  secret: '',
  isVisibleWithoutPurchase: false,
  location: null as MapCoordinates | null,
};

@Component({
  imports: [FormField, ImageUpload, LocationMap, RouterLink],
  selector: 'app-tour-key-points',
  styleUrl: './tour-key-points.scss',
  templateUrl: './tour-key-points.html',
})
export class TourKeyPoints {
  private readonly tourAuthoring = inject(TourAuthoring);

  // The tour ID from the route mine/:id/key-points.
  readonly id = input.required<string>();

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly keyPoints = httpResource<KeyPointDto[]>(
    () => `/api/exploration/tours/mine/${this.id()}/key-points`,
  );

  protected readonly tour = computed(() =>
    (this.tours.value() ?? []).find((tour) => tour.id === this.id()),
  );
  protected readonly isDraft = computed(() => this.tour()?.status === 'Draft');

  // The form is open while adding (no edited key point) or editing (the edited key point).
  protected readonly formOpen = signal(false);
  protected readonly editedKeyPoint = signal<KeyPointDto | null>(null);
  // The key point waiting for the author to confirm its removal.
  protected readonly removedKeyPoint = signal<KeyPointDto | null>(null);

  protected readonly image = signal<File | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ ...EMPTY_KEY_POINT }), (path) => {
    required(path.name, { message: 'Naziv je obavezan.' });
    required(path.description, { message: 'Opis je obavezan.' });
    required(path.location, { message: 'Izaberite lokaciju na mapi.' });
  });

  // The tour as it will be after the change in progress is saved. The map draws it and
  // reports its length, and that length is what the command sends to the server.
  protected readonly mapLocations = computed<MapLocation[]>(() => {
    const removed = this.removedKeyPoint();
    const edited = this.editedKeyPoint();
    const location = this.form.location().value();
    const name = this.form.name().value();

    const locations = (this.keyPoints.value() ?? [])
      .filter((keyPoint) => keyPoint.id !== removed?.id)
      .map((keyPoint) =>
        keyPoint.id === edited?.id && location !== null
          ? { ...location, name }
          : { latitude: keyPoint.latitude, longitude: keyPoint.longitude, name: keyPoint.name },
      );
    if (this.formOpen() && edited === null && location !== null) {
      locations.push({ ...location, name: name || 'Nova ključna tačka' });
    }
    return locations.map((point, index) => ({ ...point, name: `${index + 1}. ${point.name}` }));
  });
  protected readonly pathLength = signal(0);

  protected startAdd(): void {
    this.openForm(null, { ...EMPTY_KEY_POINT });
  }

  protected startEdit(keyPoint: KeyPointDto): void {
    this.openForm(keyPoint, {
      name: keyPoint.name,
      description: keyPoint.description,
      secret: keyPoint.secret ?? '',
      isVisibleWithoutPurchase: keyPoint.isVisibleWithoutPurchase,
      location: { latitude: keyPoint.latitude, longitude: keyPoint.longitude },
    });
  }

  protected closeForm(): void {
    this.formOpen.set(false);
    this.editedKeyPoint.set(null);
    this.form().reset({ ...EMPTY_KEY_POINT });
  }

  protected startRemove(keyPoint: KeyPointDto): void {
    this.error.set(null);
    this.removedKeyPoint.set(keyPoint);
  }

  protected cancelRemove(): void {
    this.removedKeyPoint.set(null);
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    const location = this.form.location().value();
    const edited = this.editedKeyPoint();
    const image = this.image();
    if (location === null) {
      return;
    }
    if (edited === null && image === null) {
      this.error.set('Izaberite sliku ključne tačke.');
      return;
    }
    const secret = this.form.secret().value().trim();
    const dto = {
      name: this.form.name().value(),
      description: this.form.description().value(),
      secret: secret === '' ? null : secret,
      latitude: location.latitude,
      longitude: location.longitude,
      isVisibleWithoutPurchase: this.form.isVisibleWithoutPurchase().value(),
      tourLengthInKm: this.pathLength(),
    };
    this.pending.set(true);
    this.error.set(null);
    try {
      if (edited === null && image !== null) {
        await this.tourAuthoring.addKeyPoint(this.id(), dto, image);
      } else if (edited !== null) {
        await this.tourAuthoring.updateKeyPoint(this.id(), edited.id, dto, image);
      }
      this.keyPoints.reload();
      this.tours.reload();
      this.closeForm();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Ključnu tačku nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async confirmRemove(): Promise<void> {
    const removed = this.removedKeyPoint();
    if (removed === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.removeKeyPoint(this.id(), removed.id, this.pathLength());
      this.keyPoints.reload();
      this.tours.reload();
      this.removedKeyPoint.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Ključnu tačku nije moguće ukloniti.'));
    } finally {
      this.pending.set(false);
    }
  }

  private openForm(keyPoint: KeyPointDto | null, values: typeof EMPTY_KEY_POINT): void {
    this.error.set(null);
    this.image.set(null);
    this.editedKeyPoint.set(keyPoint);
    this.form().reset(values);
    this.formOpen.set(true);
  }
}

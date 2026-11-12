import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly selectedTourId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private readonly model = signal({ transport: 'Walking' as TransportMode, minutes: 30 });

  protected readonly form = form(this.model, (path) => {
    required(path.minutes, { message: 'Minutes are required.' });
    min(path.minutes, 1, { message: 'Minutes must be at least 1.' });
  });

  protected select(tourId: string): void {
    this.error.set(null);
    this.selectedTourId.set(tourId);
    this.form().reset({ transport: 'Walking', minutes: 30 });
  }

  protected async publish(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not publish the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.archive(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not archive the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async reactivate(tourId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.reactivate(tourId);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not reactivate the tour.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected formatTime(value: string): string {
    const date = new Date(value);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${day}.${month}.${date.getFullYear()}. ${hours}:${minutes}`;
  }

  protected async addTransportTime(event: Event): Promise<void> {
    event.preventDefault();
    const tourId = this.selectedTourId();
    if (tourId === null) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.addTransportTime(tourId, {
        transport: this.form.transport().value(),
        minutes: this.form.minutes().value(),
      });
      this.tours.reload();
      this.selectedTourId.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the transport time.'));
    } finally {
      this.pending.set(false);
    }
  }
}

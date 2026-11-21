import { Component, computed, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  TourRelation,
  Transport,
  difficultyClasses,
  difficultyLabels,
  tours,
  transportLabels,
} from '../mock';
import { ProtoMap, ProtoPin, ProtoPoint } from '../proto-map/proto-map';

const RELATIONS: { value: TourRelation; label: string }[] = [
  { value: 'none', label: 'Nije kupljena' },
  { value: 'inCart', label: 'U korpi' },
  { value: 'purchased', label: 'Kupljena' },
  { value: 'inProgress', label: 'U toku' },
  { value: 'author', label: 'Vaša tura' },
];

// PROTOTYPE of the tour details (scenarios S1–S3). Content on the left; on the right a panel
// that always answers "what can I do with this tour now", and the map under it. The switcher on
// top exists only in the prototype, to show every state of the panel.
@Component({
  imports: [ProtoMap, RouterLink],
  selector: 'app-tourist-tour-details',
  styleUrl: './tourist-tour-details.scss',
  templateUrl: './tourist-tour-details.html',
})
export class TouristTourDetails {
  readonly id = input.required<string>();

  protected readonly relations = RELATIONS;
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;

  protected readonly tour = computed(() => tours.find((tour) => tour.id === this.id()) ?? tours[1]);

  // The mock's relation until the switcher or a button in the panel changes it.
  private readonly chosenRelation = signal<TourRelation | null>(null);
  protected readonly relation = computed(() => this.chosenRelation() ?? this.tour().relation);
  protected readonly position = signal<ProtoPoint | null>(null);
  protected readonly justAdded = signal(false);
  protected readonly hovered = signal<string | null>(null);

  protected readonly sees = computed(
    () => this.relation() !== 'none' && this.relation() !== 'inCart',
  );

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.tour()
      .keyPoints.map((point, index) => ({ point, index }))
      .filter(({ point }) => this.sees() || point.isOpen)
      .map(({ point, index }) => ({
        id: String(index),
        latitude: point.latitude,
        longitude: point.longitude,
        label: String(index + 1),
        tone: 'tour' as const,
      })),
  );

  protected readonly path = computed(() => (this.sees() ? this.tour().keyPoints : []));

  protected readonly facts = computed(() => {
    const tour = this.tour();
    const times = (Object.keys(tour.times) as Transport[]).map(
      (mode) => `${transportLabels[mode]} ${tour.times[mode]} min`,
    );
    return [
      `${tour.lengthKm.toFixed(1).replace('.', ',')} km`,
      ...times,
      `${tour.groupMin}–${tour.groupMax} osoba`,
    ];
  });

  protected readonly lockedCount = computed(
    () => this.tour().keyPoints.filter((point) => !point.isOpen).length,
  );

  protected setRelation(value: TourRelation): void {
    this.chosenRelation.set(value);
    this.justAdded.set(false);
  }

  protected addToCart(): void {
    this.chosenRelation.set('inCart');
    this.justAdded.set(true);
  }

  protected setPosition(point: ProtoPoint): void {
    if (this.relation() === 'purchased') {
      this.position.set(point);
    }
  }
}

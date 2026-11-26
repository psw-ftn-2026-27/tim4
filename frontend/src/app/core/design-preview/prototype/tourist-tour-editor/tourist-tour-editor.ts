import { Component, computed, input, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { ImageUpload } from '../../../../shared/image-upload/image-upload';
import { metersBetween } from '../geo';
import {
  AuthorTour,
  Difficulty,
  Transport,
  authorTours,
  difficultyLabels,
  pointsText,
  transportLabels,
} from '../mock';
import { ProtoMap, ProtoPin, ProtoPoint } from '../proto-map/proto-map';
import { readiness } from '../tourist-studio/tourist-studio';

type Step = 'basics' | 'points' | 'times' | 'publish';

const NEW_TOUR: AuthorTour = {
  id: 'new',
  name: '',
  description: '',
  difficulty: 'Easy',
  tags: [],
  groupMin: 1,
  groupMax: 8,
  status: 'Draft',
  price: null,
  times: {},
  keyPoints: [],
  image: '/images/ilustracije/adventure-map.svg',
  sold: 0,
  changedAt: '21.11.2026.',
};

// PROTOTYPE of scenario S6, the tour editor. Today the author's work is spread over "Create
// tour", the key points page and two forms under the "Moje ture" table. Here one tour is one
// page with four steps on the left; each step says whether it is done, and "Objava" lists what
// the server will check before publishing.
@Component({
  imports: [FormField, ImageUpload, ProtoMap, RouterLink],
  selector: 'app-tourist-tour-editor',
  styleUrl: './tourist-tour-editor.scss',
  templateUrl: './tourist-tour-editor.html',
})
export class TouristTourEditor {
  readonly id = input.required<string>();

  protected readonly studio = '/design-preview/prototype/tourist/studio';
  protected readonly difficulties: Difficulty[] = ['Easy', 'Moderate', 'Hard'];
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly modes: Transport[] = ['Walking', 'Bicycle', 'Car'];
  protected readonly transportLabels = transportLabels;

  private readonly source = computed(
    () => authorTours.find((tour) => tour.id === this.id()) ?? NEW_TOUR,
  );
  // The tour as the author changed it in this visit; null until the first change.
  private readonly edited = signal<AuthorTour | null>(null);
  protected readonly tour = computed(() => this.edited() ?? this.source());
  protected readonly isNew = computed(() => this.tour().id === 'new');
  protected readonly isDraft = computed(() => this.tour().status === 'Draft');

  private readonly chosenStep = signal<Step | null>(null);
  protected readonly step = computed<Step>(
    () => this.chosenStep() ?? (this.isNew() ? 'basics' : 'points'),
  );
  protected readonly notice = signal<string | null>(null);

  protected readonly checks = computed(() => readiness(this.tour()));
  protected readonly canPublish = computed(() => this.checks().every((check) => check.done));

  protected readonly steps = computed(() => {
    const tour = this.tour();
    return [
      {
        value: 'basics' as Step,
        label: 'Osnovno',
        hint: tour.name === '' ? 'Naziv, opis, težina' : tour.name,
        done: tour.name !== '' && tour.description.length >= 100,
      },
      {
        value: 'points' as Step,
        label: 'Ključne tačke',
        hint: pointsText(tour.keyPoints.length),
        done: tour.keyPoints.length >= 2,
      },
      {
        value: 'times' as Step,
        label: 'Vreme obilaska',
        hint:
          Object.keys(tour.times).length === 0
            ? 'Nije uneto'
            : (Object.keys(tour.times) as Transport[]).map((m) => transportLabels[m]).join(', '),
        done: Object.keys(tour.times).length > 0,
      },
      {
        value: 'publish' as Step,
        label: 'Cena i objava',
        hint: tour.status === 'Draft' ? 'U pripremi' : 'Objavljena',
        done: tour.status !== 'Draft',
      },
    ];
  });

  // ---- Basics ----

  protected readonly basics = form(
    signal({
      name: '',
      description: '',
      difficulty: 'Easy' as Difficulty,
      tags: '',
      groupMin: 1 as number | null,
      groupMax: 8 as number | null,
    }),
  );
  private basicsLoadedFor: string | null = null;

  protected openStep(step: Step): void {
    this.chosenStep.set(step);
    this.notice.set(null);
    if (step === 'basics' && this.basicsLoadedFor !== this.tour().id) {
      const tour = this.tour();
      this.basics().reset({
        name: tour.name,
        description: tour.description,
        difficulty: tour.difficulty,
        tags: tour.tags.join(', '),
        groupMin: tour.groupMin,
        groupMax: tour.groupMax,
      });
      this.basicsLoadedFor = tour.id;
    }
  }

  protected saveBasics(event: Event): void {
    event.preventDefault();
    const value = this.basics().value();
    const wasNew = this.isNew();
    this.update({
      id: wasNew ? 'nova-tura' : this.tour().id,
      name: value.name.trim(),
      description: value.description.trim(),
      difficulty: value.difficulty,
      tags: value.tags
        .split(',')
        .map((tag) => tag.trim())
        .filter((tag) => tag !== ''),
      groupMin: value.groupMin ?? 1,
      groupMax: value.groupMax ?? 1,
    });
    this.basicsLoadedFor = this.tour().id;
    if (wasNew) {
      this.chosenStep.set('points');
      this.notice.set('Tura je napravljena kao nacrt. Sada dodajte ključne tačke.');
    } else {
      this.notice.set('Osnovni podaci su sačuvani.');
    }
  }

  // ---- Key points ----

  // The point open for editing, by index; -1 while a new point waits for its place on the map.
  protected readonly editing = signal<number | null>(null);
  protected readonly removing = signal<number | null>(null);
  protected readonly hovered = signal<string | null>(null);
  protected readonly pointForm = form(
    signal({ name: '', description: '', secret: '', isOpen: false }),
  );

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.tour().keyPoints.map((point, index) => ({
      id: String(index),
      latitude: point.latitude,
      longitude: point.longitude,
      label: `${index + 1}${point.isOpen ? '' : ' 🔒'}`,
      tone: point.isOpen ? 'tour' : 'muted',
    })),
  );

  // The pin under the mouse, or the point open for editing.
  protected readonly mapActive = computed(() => {
    const editing = this.editing();
    return this.hovered() ?? (editing !== null && editing >= 0 ? String(editing) : null);
  });

  protected readonly lengthKm = computed(() => {
    const points = this.tour().keyPoints;
    let meters = 0;
    for (let i = 1; i < points.length; i++) {
      meters += metersBetween(points[i - 1], points[i]);
    }
    return (meters / 1000).toFixed(1).replace('.', ',');
  });

  protected startAdd(): void {
    this.editing.set(-1);
    this.removing.set(null);
  }

  protected placePoint(point: ProtoPoint): void {
    const editing = this.editing();
    if (editing === null) {
      return;
    }
    if (editing === -1) {
      const points = this.tour().keyPoints;
      this.update({
        keyPoints: [
          ...points,
          {
            name: `Tačka ${points.length + 1}`,
            description: '',
            latitude: point.latitude,
            longitude: point.longitude,
            image: '/images/ilustracije/destination.svg',
            isOpen: points.length === 0,
          },
        ],
      });
      this.edit(points.length);
    } else {
      this.update({
        keyPoints: this.tour().keyPoints.map((p, i) => (i === editing ? { ...p, ...point } : p)),
      });
    }
  }

  protected edit(index: number): void {
    const point = this.tour().keyPoints[index];
    this.pointForm().reset({
      name: point.name,
      description: point.description,
      secret: '',
      isOpen: point.isOpen,
    });
    this.editing.set(index);
    this.removing.set(null);
  }

  protected savePoint(event: Event): void {
    event.preventDefault();
    const index = this.editing();
    const value = this.pointForm().value();
    this.update({
      keyPoints: this.tour().keyPoints.map((p, i) =>
        i === index
          ? { ...p, name: value.name, description: value.description, isOpen: value.isOpen }
          : p,
      ),
    });
    this.editing.set(null);
  }

  protected move(index: number, by: number): void {
    const points = [...this.tour().keyPoints];
    const [point] = points.splice(index, 1);
    points.splice(index + by, 0, point);
    this.update({ keyPoints: points });
  }

  protected remove(index: number): void {
    this.update({ keyPoints: this.tour().keyPoints.filter((_, i) => i !== index) });
    this.removing.set(null);
    this.editing.set(null);
  }

  // ---- Times ----

  protected readonly timeForm = form(
    signal({
      Walking: null as number | null,
      Bicycle: null as number | null,
      Car: null as number | null,
    }),
  );

  protected saveTime(mode: Transport): void {
    const minutes = this.timeForm().value()[mode];
    if (minutes === null || Number.isNaN(minutes) || minutes <= 0) {
      return;
    }
    this.update({ times: { ...this.tour().times, [mode]: minutes } });
  }

  // ---- Publish ----

  protected readonly pricing = form(
    signal({ type: '' as '' | 'Free' | 'Paid', amount: null as number | null }),
  );

  protected publish(): void {
    const value = this.pricing().value();
    const price = value.type === 'Free' ? 0 : (value.amount ?? 0);
    this.update({ price, status: 'Published' });
    this.notice.set('Tura je objavljena. Turisti je od sada vide u pretrazi.');
  }

  // The pricing is part of the checklist, so it is applied as soon as it is chosen.
  protected choosePricing(): void {
    const value = this.pricing().value();
    const price =
      value.type === ''
        ? null
        : value.type === 'Free'
          ? 0
          : value.amount !== null && value.amount > 0
            ? value.amount
            : null;
    this.update({ price });
  }

  private update(changes: Partial<AuthorTour>): void {
    this.edited.set({ ...this.tour(), ...changes });
  }
}

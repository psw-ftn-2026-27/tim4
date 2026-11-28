import { Component, computed, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import {
  AuthorTour,
  TourStatus,
  authorTours,
  difficultyClasses,
  difficultyLabels,
  pointsText,
} from '../mock';

type Tab = TourStatus | 'all' | 'coupons';

interface Coupon {
  code: string;
  percent: number;
  until: string;
  tour: string;
  valid: boolean;
}

const STATUS: Record<TourStatus, { label: string; badge: string }> = {
  Draft: { label: '✎ U pripremi', badge: 'badge badge--draft' },
  Published: { label: '● Objavljena', badge: 'badge badge--published' },
  Archived: { label: '▣ Arhivirana', badge: 'badge badge--archived' },
};

// What a draft still needs before it can be published, as the server checks it in Tour.Publish.
export function readiness(tour: AuthorTour): { label: string; done: boolean }[] {
  return [
    { label: 'Opis od bar 100 znakova', done: tour.description.length >= 100 },
    { label: 'Bar 2 ključne tačke', done: tour.keyPoints.length >= 2 },
    { label: 'Vreme obilaska za bar jedan prevoz', done: Object.keys(tour.times).length > 0 },
    { label: 'Cena (besplatna ili u EC)', done: tour.price !== null },
  ];
}

// PROTOTYPE of scenarios S6–S7. Today "Moje ture" is a seven-column table with forms that open
// under it, and the author's coupons have no link at all. Here the studio is the author's home:
// tours by state, what each draft still lacks, and the coupons as a tab of the same place.
@Component({
  imports: [FormField, RouterLink],
  selector: 'app-tourist-studio',
  styleUrl: './tourist-studio.scss',
  templateUrl: './tourist-studio.html',
})
export class TouristStudio {
  protected readonly status = STATUS;
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly base = '/design-preview/prototype/tourist/studio';
  protected readonly pointsText = pointsText;

  protected readonly items = signal<AuthorTour[]>(authorTours.map((tour) => ({ ...tour })));
  protected readonly tab = signal<Tab>('all');
  protected readonly notice = signal<string | null>(null);

  protected readonly tabs = computed(() => {
    const items = this.items();
    const count = (status: TourStatus): number => items.filter((t) => t.status === status).length;
    return [
      { value: 'all' as Tab, label: 'Sve ture', count: items.length },
      { value: 'Draft' as Tab, label: 'U pripremi', count: count('Draft') },
      { value: 'Published' as Tab, label: 'Objavljene', count: count('Published') },
      { value: 'Archived' as Tab, label: 'Arhivirane', count: count('Archived') },
      { value: 'coupons' as Tab, label: 'Kuponi', count: this.coupons().length },
    ];
  });

  protected readonly visible = computed(() =>
    this.items().filter((tour) => this.tab() === 'all' || tour.status === this.tab()),
  );

  protected readonly sold = computed(() => this.items().reduce((sum, t) => sum + t.sold, 0));

  // ---- Coupons ----

  protected readonly coupons = signal<Coupon[]>([
    { code: 'JESEN20', percent: 20, until: '30.11.2026.', tour: 'Sve moje ture', valid: true },
    {
      code: 'VINO10',
      percent: 10,
      until: '01.10.2026.',
      tour: 'Vino i priče Sremskih Karlovaca',
      valid: false,
    },
  ]);
  protected readonly couponFormOpen = signal(false);
  protected readonly couponForm = form(
    signal({ percent: 15 as number | null, until: '2026-12-31', tour: '' }),
  );
  protected readonly publishedTours = computed(() =>
    this.items().filter((tour) => tour.status === 'Published'),
  );

  protected readiness(tour: AuthorTour): { label: string; done: boolean }[] {
    return readiness(tour);
  }

  protected readyCount(tour: AuthorTour): number {
    return readiness(tour).filter((item) => item.done).length;
  }

  protected archive(tour: AuthorTour): void {
    this.setStatus(
      tour,
      'Archived',
      `„${tour.name}“ je arhivirana: turisti je više ne vide u pretrazi.`,
    );
  }

  protected reactivate(tour: AuthorTour): void {
    this.setStatus(tour, 'Published', `„${tour.name}“ je ponovo objavljena.`);
  }

  protected createCoupon(event: Event): void {
    event.preventDefault();
    const value = this.couponForm().value();
    const code = 'NOVI' + (value.percent ?? 0);
    const [year, month, day] = value.until.split('-');
    this.coupons.update((list) => [
      {
        code,
        percent: value.percent ?? 0,
        until: `${day}.${month}.${year}.`,
        tour: value.tour === '' ? 'Sve moje ture' : value.tour,
        valid: true,
      },
      ...list,
    ]);
    this.couponFormOpen.set(false);
    this.notice.set(`Kupon ${code} je napravljen. Pošaljite kod turistima; unose ga pri plaćanju.`);
  }

  private setStatus(tour: AuthorTour, status: TourStatus, message: string): void {
    this.items.update((items) => items.map((t) => (t.id === tour.id ? { ...t, status } : t)));
    this.notice.set(message);
  }
}

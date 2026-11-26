import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatDate, formatKm } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TourStatus } from '../../api/exploration-api-types';
import { difficultyBadges, difficultyLabels } from '../../tour-browsing/tour-card/tour-card';
import { readiness } from '../editor-steps/editor-steps';
import { TourAuthoring } from '../tour-authoring';

type Tab = TourStatus | 'all';

const STATUS: Record<TourStatus, { label: string; badge: string }> = {
  Draft: { label: '✎ U pripremi', badge: 'badge badge--draft' },
  Published: { label: '● Objavljena', badge: 'badge badge--published' },
  Archived: { label: '▣ Arhivirana', badge: 'badge badge--archived' },
};

// The author's home: their tours by state, what each draft still lacks before it can be
// published, and the one next action for each. The coupons are the author's too, but they are
// Payment's page, so the last tab is a link to it.
@Component({
  imports: [RouterLink],
  selector: 'app-my-tours',
  styleUrl: './my-tours.scss',
  templateUrl: './my-tours.html',
})
export class MyTours {
  private readonly tourAuthoring = inject(TourAuthoring);

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');

  protected readonly status = STATUS;
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyBadges = difficultyBadges;
  protected readonly formatKm = formatKm;

  protected readonly tab = signal<Tab>('all');
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  // The published tour whose archiving waits for the author's confirmation.
  protected readonly archiving = signal<string | null>(null);

  protected readonly tabs = computed(() => {
    const tours = this.tours.value() ?? [];
    const count = (status: TourStatus) => tours.filter((tour) => tour.status === status).length;
    return [
      { value: 'all' as Tab, label: 'Sve', count: tours.length },
      { value: 'Draft' as Tab, label: 'U pripremi', count: count('Draft') },
      { value: 'Published' as Tab, label: 'Objavljene', count: count('Published') },
      { value: 'Archived' as Tab, label: 'Arhivirane', count: count('Archived') },
    ];
  });

  // Drafts first, as they are what the author is working on.
  protected readonly visible = computed(() => {
    const order: Record<TourStatus, number> = { Draft: 0, Published: 1, Archived: 2 };
    return (this.tours.value() ?? [])
      .filter((tour) => this.tab() === 'all' || tour.status === this.tab())
      .sort((a, b) => order[a.status] - order[b.status]);
  });

  protected readiness(tour: TourDto) {
    return readiness(tour, null);
  }

  protected readyCount(tour: TourDto): number {
    return readiness(tour, null).filter((item) => item.done).length;
  }

  protected changed(tour: TourDto): string {
    const date = tour.archivedAt ?? tour.publishedAt;
    return date === null ? '' : formatDate(date);
  }

  protected async archive(tour: TourDto): Promise<void> {
    await this.run(
      () => this.tourAuthoring.archive(tour.id),
      `„${tour.name}“ je arhivirana: turisti je više ne vide u pretrazi, a kupci je i dalje imaju.`,
      'Tura nije arhivirana. Pokušajte ponovo.',
    );
    this.archiving.set(null);
  }

  protected async reactivate(tour: TourDto): Promise<void> {
    await this.run(
      () => this.tourAuthoring.reactivate(tour.id),
      `„${tour.name}“ je ponovo objavljena.`,
      'Tura nije ponovo objavljena. Pokušajte ponovo.',
    );
  }

  private async run(command: () => Promise<void>, success: string, fallback: string) {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await command();
      this.notice.set(success);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, fallback));
    } finally {
      this.pending.set(false);
    }
  }
}

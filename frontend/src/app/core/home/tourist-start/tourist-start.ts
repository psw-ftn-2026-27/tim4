import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import type {
  ActiveTourDto,
  PublishedTourDto,
  TourDifficulty,
  TourPreferenceDto,
} from '../../../modules/exploration/api/exploration-api-types';
import type {
  LocationChallengeDto,
  QuizMapItemDto,
  SocialChallengeMapItemDto,
} from '../../../modules/games/api/games-api-types';
import type { GiftCardDto, WalletBalanceDto } from '../../../modules/payment/api/payment-api-types';
import { CoordinatesDto } from '../../../shared/api/coordinates-dto';
import { PageResult } from '../../../shared/api/page-result';
import { formatKm } from '../../../shared/util/format';

const DIFFICULTY: Record<TourDifficulty, { label: string; badge: string }> = {
  Easy: { label: 'Lako', badge: 'badge badge--easy' },
  Moderate: { label: 'Srednje', badge: 'badge badge--moderate' },
  Hard: { label: 'Teško', badge: 'badge badge--hard' },
};

// The tourist's start page answers "what now?". A new tourist gets the first steps, with the
// two things nobody guesses (EC come from the administrator, the position is a click on the
// map), each ticked from live data; a returning tourist gets the tour in progress, the
// challenges near them, the wallet and tours to try. Like the bar, this is a place outside the
// modules that reads their endpoints (frontend/AGENTS.md, "The start page").
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-start',
  styleUrl: './tourist-start.scss',
  templateUrl: './tourist-start.html',
})
export class TouristStart {
  readonly name = input.required<string>();
  // True right after registration (?novi=1).
  readonly justRegistered = input(false);

  protected readonly difficulty = DIFFICULTY;
  protected readonly kindLabels: Record<string, string> = {
    '?': 'Kviz',
    '☺': 'Društveni izazov',
    '⌖': 'Lokacijski izazov',
  };

  // 204 No Content (nothing in progress, no position, no preference) reads as null.
  private readonly activeTour = httpResource<ActiveTourDto | null>(
    () => '/api/exploration/tour-executions/active',
  );
  private readonly wallet = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');
  private readonly cards = httpResource<PageResult<GiftCardDto>>(
    () => '/api/payment/gift-cards/received?page=1&pageSize=50',
  );
  private readonly position = httpResource<CoordinatesDto | null>(() => '/api/positions/mine');
  private readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/preferences',
  );
  private readonly tours = httpResource<PageResult<PublishedTourDto>>(
    () => `/api/exploration/tours/published?${this.tourQuery()}`,
  );
  private readonly quizzes = httpResource<QuizMapItemDto[]>(() => '/api/games/quizzes/map');
  private readonly social = httpResource<SocialChallengeMapItemDto[]>(
    () => '/api/games/social-challenges/map',
  );
  private readonly locations = httpResource<PageResult<LocationChallengeDto>>(
    () => '/api/games/location-challenges?page=1&pageSize=50',
  );

  protected readonly active = computed(() =>
    this.activeTour.hasValue() ? this.activeTour.value() : null,
  );
  protected readonly balance = computed(() =>
    this.wallet.hasValue() ? this.wallet.value().availableBalance : null,
  );
  protected readonly savedPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );
  protected readonly savedPreference = computed(() =>
    this.preference.hasValue() ? this.preference.value() : null,
  );
  protected readonly giftCard = computed(
    () => (this.cards.value()?.items ?? []).find((card) => card.status === 'Valid') ?? null,
  );

  // A tourist is new right after registration, and also while they have no EC and nothing in
  // progress: the first steps are what they need then.
  protected readonly isNew = computed(
    () =>
      this.justRegistered() ||
      (this.balance() === 0 && this.activeTour.hasValue() && this.active() === null),
  );

  protected readonly steps = computed(() => [
    {
      title: 'Nalog je napravljen',
      text: 'Prijavljeni ste kao turista.',
      done: true,
      link: null as string | null,
      action: '',
    },
    {
      title: 'Nabavite EC',
      text: 'Ture se plaćaju u EC (Explorer coin). EC uplaćuje administrator; do tada možete da uzmete besplatnu turu.',
      done: (this.balance() ?? 0) > 0,
      link: '/payment',
      action: 'Novčanik',
    },
    {
      title: 'Izaberite turu',
      text: 'Pretražite ture po težini, prevozu i ceni, na spisku ili na mapi.',
      done: this.active() !== null,
      link: '/exploration',
      action: 'Pretraži ture',
    },
    {
      title: 'Postavite poziciju na mapi',
      text: 'Aplikacija ne koristi GPS: na stranici ture ili izazova kliknite na mapu tamo gde se nalazite. Tako zna da ste stigli do tačke.',
      done: this.savedPosition() !== null,
      link: null,
      action: '',
    },
    {
      title: 'Sačuvajte preference (nije obavezno)',
      text: 'Pretraga će odmah krenuti od tura koje volite.',
      done: this.savedPreference() !== null,
      link: '/exploration/preferences',
      action: 'Preference',
    },
  ]);
  protected readonly doneCount = computed(() => this.steps().filter((step) => step.done).length);

  // Tours to try: the ones matching the saved preference when there is one, free ones first,
  // never the tour already in progress.
  protected readonly suggested = computed(() => {
    const activeId = this.active()?.tourId;
    return [...(this.tours.value()?.items ?? [])]
      .filter((item) => item.tour.id !== activeId && item.price !== null)
      .sort((a, b) => (a.price ?? 0) - (b.price ?? 0))
      .slice(0, 3);
  });
  protected readonly suggestedTitle = computed(() =>
    this.savedPreference() === null ? 'Za početak' : 'Po vašim preferencama',
  );

  // The unsolved challenges of all three kinds, nearest first once the position is known.
  protected readonly near = computed(() => {
    const position = this.savedPosition();
    const all = [
      ...(this.quizzes.value() ?? [])
        .filter((quiz) => quiz.progress !== 'Solved')
        .map((quiz) => ({ id: quiz.id, name: quiz.name, mark: '?', ...point(quiz) })),
      ...(this.social.value() ?? [])
        .filter((challenge) => !challenge.solved)
        .map((challenge) => ({
          id: challenge.id,
          name: challenge.name,
          mark: '☺',
          ...point(challenge),
        })),
      ...(this.locations.value()?.items ?? []).map((challenge) => ({
        id: challenge.id,
        name: challenge.name,
        mark: '⌖',
        latitude: challenge.startLatitude,
        longitude: challenge.startLongitude,
      })),
    ].map((item) => ({
      ...item,
      meters: position === null ? null : metersBetween(position, item),
    }));
    return all.sort((a, b) => (a.meters ?? 0) - (b.meters ?? 0)).slice(0, 3);
  });

  protected readonly progress = computed(() => {
    const active = this.active();
    return active === null
      ? '0%'
      : `${(active.reachedKeyPointCount / active.keyPoints.length) * 100}%`;
  });

  protected distance(meters: number | null): string {
    return meters === null ? '' : formatKm(meters / 1000);
  }

  // The saved preference narrows the suggestions, the same way the tour search applies it.
  private tourQuery(): string {
    const query = new URLSearchParams({ page: '1', pageSize: '20', sort: 'PriceAscending' });
    const preference = this.preference.hasValue() ? this.preference.value() : null;
    for (const difficulty of preference?.difficulties ?? []) {
      query.append('difficulties', difficulty);
    }
    for (const tag of preference?.tags ?? []) {
      query.append('tags', tag);
    }
    if (preference?.transport) {
      query.append('transport', preference.transport);
    }
    return query.toString();
  }
}

function point(item: { latitude: number; longitude: number }) {
  return { latitude: item.latitude, longitude: item.longitude };
}

// The distance between two points on the Earth, in metres (the haversine formula).
function metersBetween(
  a: { latitude: number; longitude: number },
  b: { latitude: number; longitude: number },
): number {
  const rad = (degrees: number) => (degrees * Math.PI) / 180;
  const h =
    Math.sin(rad(b.latitude - a.latitude) / 2) ** 2 +
    Math.cos(rad(a.latitude)) *
      Math.cos(rad(b.latitude)) *
      Math.sin(rad(b.longitude - a.longitude) / 2) ** 2;
  return 6371000 * 2 * Math.atan2(Math.sqrt(h), Math.sqrt(1 - h));
}

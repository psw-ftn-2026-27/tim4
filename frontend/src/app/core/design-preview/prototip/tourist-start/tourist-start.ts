import { Component, computed, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { distanceText, metersBetween } from '../geo';
import {
  activeReachedCount,
  activeTourId,
  difficultyClasses,
  difficultyLabels,
  playChallenges,
  pointsText,
  playerPosition,
  playerXp,
  readyTourIds,
  receivedCards,
  tours,
  walletBalance,
} from '../mock';

// PROTOTYPE of the tourist's start page. Today it is four link boxes that repeat the bar and lead
// to Social. Here it answers "what now?": a new tourist gets the first steps, including the two
// things nobody guesses (EC come from the administrator, the position is a click on the map);
// a returning tourist gets what is in progress, what is ready and what is near.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-start',
  styleUrl: './tourist-start.scss',
  templateUrl: './tourist-start.html',
})
export class TouristStart {
  // ?novi=1 after registration.
  readonly novi = input<string | undefined>(undefined);
  private readonly chosen = signal<boolean | null>(null);
  protected readonly isNew = computed(() => this.chosen() ?? this.novi() !== undefined);

  protected readonly base = '/design-preview/prototype/tourist';
  protected readonly pointsText = pointsText;
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly xp = playerXp;
  protected readonly balance = walletBalance;
  protected readonly giftCard = receivedCards.find((card) => card.status === 'Valid') ?? null;

  protected readonly active = tours.find((tour) => tour.id === activeTourId)!;
  protected readonly activeReached = activeReachedCount;
  protected readonly ready = readyTourIds
    .map((id) => tours.find((tour) => tour.id === id)!)
    .slice(0, 2);

  protected readonly near = playChallenges
    .filter((c) => c.status !== 'solved')
    .map((c) => ({ challenge: c, meters: metersBetween(playerPosition, c) }))
    .sort((a, b) => a.meters - b.meters)
    .slice(0, 3);

  // Free tours first for someone who has no EC yet.
  protected readonly suggested = [...tours]
    .filter((tour) => tour.relation === 'none' || tour.price === 0)
    .sort((a, b) => a.price - b.price)
    .slice(0, 3);

  protected readonly steps = [
    {
      title: 'Nalog je napravljen',
      text: 'Prijavljeni ste kao turista.',
      done: true,
      link: null as string | null,
      action: '',
    },
    {
      title: 'Nabavite EC',
      text: 'Ture se plaćaju u EC (Explorer coin). Prve EC uplaćuje administrator; do tada možete da uzmete besplatnu turu.',
      done: false,
      link: '/wallet',
      action: 'Novčanik',
    },
    {
      title: 'Izaberite turu',
      text: 'Pretražite ture po težini, prevozu i ceni ili ih pogledajte na mapi.',
      done: false,
      link: '/tours',
      action: 'Pretraži ture',
    },
    {
      title: 'Postavite poziciju na mapi',
      text: 'Aplikacija ne koristi GPS: na stranici ture kliknite na mapu tamo gde se nalazite. Tako zna da ste stigli do tačke ili izazova.',
      done: false,
      link: null,
      action: '',
    },
    {
      title: 'Sačuvajte preference (nije obavezno)',
      text: 'Pretraga će odmah krenuti od tura koje volite.',
      done: false,
      link: '/preferences',
      action: 'Preference',
    },
  ];

  protected setNew(value: boolean): void {
    this.chosen.set(value);
  }

  protected distance(meters: number): string {
    return distanceText(meters);
  }
}

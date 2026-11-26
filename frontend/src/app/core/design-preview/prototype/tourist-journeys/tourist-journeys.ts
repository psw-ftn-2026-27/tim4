import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  activeReachedCount,
  activeTourId,
  difficultyClasses,
  difficultyLabels,
  history,
  readyTourIds,
  secrets,
  tours,
} from '../mock';

type Tab = 'ready' | 'history';

// PROTOTYPE of scenario S3, the tourist's own tours. Today a bought tour exists only as a flag on
// its details page. Here it has a home: the tour in progress on top, then the bought tours ready
// to start and the history with every secret the tourist found.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-journeys',
  styleUrl: './tourist-journeys.scss',
  templateUrl: './tourist-journeys.html',
})
export class TouristJourneys {
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly tab = signal<Tab>('ready');

  protected readonly active = tours.find((tour) => tour.id === activeTourId)!;
  protected readonly activeReached = activeReachedCount;
  protected readonly activeNext = this.active.keyPoints[activeReachedCount];

  protected readonly ready = readyTourIds.map((id) => tours.find((tour) => tour.id === id)!);

  protected readonly past = history.map((execution) => {
    const tour = tours.find((t) => t.id === execution.tourId)!;
    return {
      ...execution,
      tour,
      found: (secrets[tour.id] ?? []).slice(0, execution.reached).map((secret, index) => ({
        point: tour.keyPoints[index].name,
        secret,
      })),
    };
  });

  protected readonly tabs = computed(() => [
    { value: 'ready' as Tab, label: 'Spremne za polazak', count: this.ready.length },
    { value: 'history' as Tab, label: 'Istorija', count: this.past.length },
  ]);

  protected progress(reached: number, total: number): string {
    return `${(reached / total) * 100}%`;
  }
}

import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TourDto } from '../../api/exploration-api-types';

export type EditorStep = 'basics' | 'points' | 'times' | 'publish';

// What a draft needs before the server lets it be published (Tour.Publish): a description of
// at least 100 characters, at least two key points and a time for at least one way of travel.
// Without the key points' count (the studio list does not load them) a tour with a length has
// at least two. The price is chosen in the last step, when publishing.
export function readiness(
  tour: TourDto,
  keyPointCount: number | null,
): { step: EditorStep; label: string; done: boolean }[] {
  const points = keyPointCount === null ? tour.lengthInKm > 0 : keyPointCount >= 2;
  return [
    { step: 'basics', label: 'Opis od bar 100 znakova', done: tour.description.length >= 100 },
    { step: 'points', label: 'Bar 2 ključne tačke', done: points },
    {
      step: 'times',
      label: 'Vreme obilaska za bar jedan prevoz',
      done: tour.transportTimes.length > 0,
    },
  ];
}

const STATUS: Record<TourDto['status'], { label: string; badge: string }> = {
  Draft: { label: '✎ U pripremi', badge: 'badge badge--draft' },
  Published: { label: '● Objavljena', badge: 'badge badge--published' },
  Archived: { label: '▣ Arhivirana', badge: 'badge badge--archived' },
};

// The head of every step of the tour editor: back to the studio, the tour's name and state,
// and the four steps, each ticked once it is ready, so the author always knows what is left.
@Component({
  imports: [RouterLink],
  selector: 'app-editor-steps',
  styleUrl: './editor-steps.scss',
  templateUrl: './editor-steps.html',
})
export class EditorSteps {
  readonly tour = input.required<TourDto>();
  readonly current = input.required<EditorStep>();
  // Known on the key points step; elsewhere the tour's length stands in for it.
  readonly keyPointCount = input<number | null>(null);

  protected readonly status = STATUS;

  protected readonly steps = computed(() => {
    const ready = readiness(this.tour(), this.keyPointCount());
    const done = (step: EditorStep) => ready.find((item) => item.step === step)?.done ?? false;
    const id = this.tour().id;
    return [
      {
        step: 'basics' as EditorStep,
        label: 'Osnovno',
        link: ['/exploration/mine', id],
        done: done('basics'),
      },
      {
        step: 'points' as EditorStep,
        label: 'Ključne tačke',
        link: ['/exploration/mine', id, 'key-points'],
        done: done('points'),
      },
      {
        step: 'times' as EditorStep,
        label: 'Vreme obilaska',
        link: ['/exploration/mine', id, 'times'],
        done: done('times'),
      },
      {
        step: 'publish' as EditorStep,
        label: 'Cena i objava',
        link: ['/exploration/mine', id, 'publish'],
        done: this.tour().status !== 'Draft',
      },
    ];
  });
}

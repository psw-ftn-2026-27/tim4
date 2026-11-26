import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

export type AdminKind = 'quiz' | 'social' | 'location';

// The head of the administrator's "Izazovi": the title with the page's one primary action, and
// the three kinds of challenge as tabs. Each tab is its own page, in its own group.
@Component({
  imports: [RouterLink],
  selector: 'app-admin-tabs',
  styleUrl: './admin-tabs.scss',
  templateUrl: './admin-tabs.html',
})
export class AdminTabs {
  readonly current = input.required<AdminKind>();
  readonly actionLabel = input.required<string>();
  readonly actionLink = input.required<string>();
  readonly counts = input<Partial<Record<AdminKind, number>>>({});

  protected readonly tabs: { kind: AdminKind; label: string; link: string }[] = [
    { kind: 'quiz', label: '? Kvizovi', link: '/games/admin' },
    { kind: 'social', label: '☺ Društveni', link: '/games/admin/social' },
    { kind: 'location', label: '⌖ Lokacijski', link: '/games/admin/location' },
  ];
}

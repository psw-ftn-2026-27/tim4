import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { distanceText, metersBetween } from '../geo';
import {
  ChallengeKind,
  PlayChallenge,
  challengeKindLabels,
  playChallenges,
  playerPosition,
  playerXp,
} from '../mock';
import { PinTone, ProtoMap, ProtoPin, ProtoPoint } from '../proto-map/proto-map';

type KindFilter = ChallengeKind | 'all';

const KIND_TONES: Record<ChallengeKind, PinTone> = {
  quiz: 'quiz',
  social: 'social',
  location: 'location',
};

export const KIND_MARKS: Record<ChallengeKind, string> = {
  quiz: '?',
  social: '☺',
  location: '⌖',
};

// PROTOTYPE of scenario S4. Today quizzes and social challenges live on one map that is not in
// the bar, and location challenges in a separate list. Here all three are one screen in the same
// vertical slice as "Ture": the list on the left, the map on the right. Choosing a challenge turns
// the list into its panel, which always says how far the tourist is and what to do next.
@Component({
  imports: [ProtoMap, RouterLink],
  selector: 'app-tourist-challenges',
  styleUrl: './tourist-challenges.scss',
  templateUrl: './tourist-challenges.html',
})
export class TouristChallenges {
  protected readonly kindLabels = challengeKindLabels;
  protected readonly marks = KIND_MARKS;

  protected readonly items = signal<PlayChallenge[]>(playChallenges.map((c) => ({ ...c })));
  protected readonly kind = signal<KindFilter>('all');
  protected readonly showSolved = signal(false);
  protected readonly selectedId = signal<string | null>(null);
  protected readonly hovered = signal<string | null>(null);
  protected readonly position = signal<ProtoPoint>(playerPosition);
  protected readonly xp = signal(playerXp);

  // Social challenge run: whether the tourist joined, and how many players are there now.
  protected readonly joined = signal(false);
  protected readonly present = signal(1);
  // Location challenge: whether it is active, and which photos were found.
  protected readonly activated = signal(false);
  protected readonly discovered = signal<number[]>([]);
  // The XP of the latest solve, for the notice.
  protected readonly justEarned = signal<number | null>(null);

  protected readonly kinds = computed(() =>
    (['all', 'quiz', 'social', 'location'] as KindFilter[]).map((kind) => ({
      kind,
      label:
        kind === 'all'
          ? 'Svi'
          : `${KIND_MARKS[kind]} ${{ quiz: 'Kvizovi', social: 'Društveni', location: 'Lokacijski' }[kind]}`,
    })),
  );

  // Open challenges first, nearest first; solved ones only when asked for, or the one just solved.
  protected readonly visible = computed(() =>
    this.items()
      .filter(
        (c) =>
          (this.kind() === 'all' || c.kind === this.kind()) &&
          (this.showSolved() || c.status !== 'solved' || c.id === this.selectedId()),
      )
      .map((c) => ({ challenge: c, meters: metersBetween(this.position(), c) }))
      .sort((a, b) => a.meters - b.meters),
  );

  protected readonly solvedCount = computed(
    () => this.items().filter((c) => c.status === 'solved').length,
  );

  protected readonly selected = computed(
    () => this.items().find((c) => c.id === this.selectedId()) ?? null,
  );

  protected readonly meters = computed(() => {
    const c = this.selected();
    return c === null ? null : metersBetween(this.position(), c);
  });
  protected readonly inRange = computed(() => {
    const c = this.selected();
    const meters = this.meters();
    return c !== null && meters !== null && meters <= c.rangeMeters;
  });

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.visible().map(({ challenge: c }) => ({
      id: c.id,
      latitude: c.latitude,
      longitude: c.longitude,
      label: c.status === 'solved' ? `✓ ${c.earnedXp} XP` : `${KIND_MARKS[c.kind]} ${c.xp} XP`,
      tone: c.status === 'solved' ? 'owned' : KIND_TONES[c.kind],
    })),
  );

  // The circle on the map: where the challenge can be started, or, for an active location
  // challenge, the larger circle the hidden points are in.
  protected readonly area = computed(() => {
    const c = this.selected();
    if (c === null || c.status === 'solved') {
      return null;
    }
    const radiusMeters =
      c.kind === 'location' && this.activated() ? (c.searchRadius ?? c.rangeMeters) : c.rangeMeters;
    return { latitude: c.latitude, longitude: c.longitude, radiusMeters };
  });

  // One place per player the social challenge needs, for the row of dots.
  protected readonly slots = computed(() =>
    Array.from({ length: this.selected()?.participants ?? 0 }, (_, index) => index),
  );

  // While the tourist is out of range of the chosen challenge, the map shows the way to it.
  protected readonly routeTarget = computed(() => {
    const c = this.selected();
    return c === null || c.status === 'solved' || this.inRange() ? null : c;
  });

  protected readonly activePin = computed(() => this.hovered() ?? this.selectedId());

  protected distance(meters: number): string {
    return distanceText(meters);
  }

  protected select(id: string | null): void {
    this.selectedId.set(id);
    this.joined.set(false);
    this.present.set(1);
    this.activated.set(false);
    this.discovered.set([]);
    this.justEarned.set(null);
  }

  // Prototype shortcut: stand in the middle of the chosen challenge.
  protected goThere(): void {
    const c = this.selected();
    if (c !== null) {
      this.position.set({ latitude: c.latitude, longitude: c.longitude });
    }
  }

  protected join(): void {
    this.joined.set(true);
    this.present.set(2);
  }

  protected playerArrives(): void {
    const c = this.selected();
    if (c === null) {
      return;
    }
    this.present.update((count) => count + 1);
    if (this.present() >= (c.participants ?? 0)) {
      this.solve(c, c.xp);
    }
  }

  protected check(index: number): void {
    const c = this.selected();
    if (c === null) {
      return;
    }
    this.discovered.update((found) => [...found, index]);
    if (this.discovered().length === (c.photos?.length ?? 0)) {
      this.solve(c, c.xp);
    }
  }

  private solve(c: PlayChallenge, earned: number): void {
    this.items.update((items) =>
      items.map((item) =>
        item.id === c.id ? { ...item, status: 'solved', earnedXp: earned } : item,
      ),
    );
    this.xp.update((xp) => xp + earned);
    this.justEarned.set(earned);
  }
}

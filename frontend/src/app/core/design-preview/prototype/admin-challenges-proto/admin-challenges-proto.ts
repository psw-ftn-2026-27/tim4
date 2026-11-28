import { Component, computed, signal } from '@angular/core';
import {
  ChallengeKind,
  ChallengeStatus,
  ProtoChallenge,
  challengeKindLabels,
  challenges,
} from '../mock';
import { PinTone, ProtoMap, ProtoPin } from '../proto-map/proto-map';

type KindTab = ChallengeKind | 'all';

const STATUS_LABELS: Record<ChallengeStatus, { label: string; badge: string }> = {
  Draft: { label: '✎ Nacrt', badge: 'badge badge--draft' },
  Published: { label: '● Objavljen', badge: 'badge badge--published' },
  Retired: { label: '⊘ Povučen', badge: 'badge badge--retired' },
  Archived: { label: '▣ Arhiviran', badge: 'badge badge--archived' },
};

const KIND_TONES: Record<ChallengeKind, PinTone> = {
  quiz: 'quiz',
  social: 'social',
  location: 'location',
};

// A transition from one state to the next, as the quiz and the social challenge allow today.
interface Transition {
  label: string;
  to: ChallengeStatus;
}

// PROTOTYPE of scenarios S9–S11. Today an administrator has three menu items and three
// different tables for quizzes, social and location challenges; here they are one list with
// tabs by kind, one set of state badges and actions, and the map beside it.
@Component({
  imports: [ProtoMap],
  selector: 'app-admin-challenges-proto',
  styleUrl: './admin-challenges-proto.scss',
  templateUrl: './admin-challenges-proto.html',
})
export class AdminChallengesProto {
  protected readonly kindLabels = challengeKindLabels;
  protected readonly statusLabels = STATUS_LABELS;
  protected readonly statuses: ChallengeStatus[] = ['Draft', 'Published', 'Retired', 'Archived'];

  // A copy, so the prototype's actions can change states.
  protected readonly items = signal<ProtoChallenge[]>(challenges.map((c) => ({ ...c })));
  protected readonly tab = signal<KindTab>('all');
  protected readonly status = signal<ChallengeStatus | null>(null);
  protected readonly hovered = signal<string | null>(null);
  protected readonly confirmingDelete = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly tabs = computed(() => {
    const items = this.items();
    const count = (kind: KindTab): number =>
      kind === 'all' ? items.length : items.filter((c) => c.kind === kind).length;
    return (['all', 'quiz', 'social', 'location'] as KindTab[]).map((kind) => ({
      kind,
      label: kind === 'all' ? 'Svi' : this.kindPlural(kind),
      count: count(kind),
    }));
  });

  protected readonly visible = computed(() =>
    this.items().filter(
      (c) =>
        (this.tab() === 'all' || c.kind === this.tab()) &&
        (this.status() === null || c.status === this.status()),
    ),
  );

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.visible().map((c) => ({
      id: c.id,
      latitude: c.latitude,
      longitude: c.longitude,
      label: `${c.kind === 'quiz' ? '?' : c.kind === 'social' ? '☺' : '⌖'} ${c.xp} XP`,
      tone: c.status === 'Published' ? KIND_TONES[c.kind] : 'muted',
    })),
  );

  // The first action moves the challenge forward, the second is the other way out of the state.
  protected transitions(c: ProtoChallenge): Transition[] {
    if (c.kind === 'location') {
      return [];
    }
    switch (c.status) {
      case 'Draft':
        return [{ label: 'Objavi', to: 'Published' }];
      case 'Published':
        return [{ label: 'Povuci', to: 'Retired' }];
      case 'Retired':
        return [
          { label: 'Objavi ponovo', to: 'Published' },
          { label: 'Arhiviraj', to: 'Archived' },
        ];
      case 'Archived':
        return [];
    }
  }

  protected canEdit(c: ProtoChallenge): boolean {
    return c.status === 'Draft';
  }

  protected canDelete(c: ProtoChallenge): boolean {
    return c.kind !== 'location' && (c.status === 'Draft' || c.status === 'Archived');
  }

  protected apply(c: ProtoChallenge, transition: Transition): void {
    this.items.update((items) =>
      items.map((item) => (item.id === c.id ? { ...item, status: transition.to } : item)),
    );
    this.notice.set(`„${c.name}“: ${STATUS_LABELS[transition.to].label.slice(2).toLowerCase()}.`);
  }

  protected remove(c: ProtoChallenge): void {
    this.items.update((items) => items.filter((item) => item.id !== c.id));
    this.confirmingDelete.set(null);
    this.notice.set(`„${c.name}“ je obrisan.`);
  }

  protected kindClass(kind: ChallengeKind): string {
    return 'kind kind--' + kind;
  }

  private kindPlural(kind: ChallengeKind): string {
    return { quiz: 'Kvizovi', social: 'Društveni', location: 'Lokacijski' }[kind];
  }
}

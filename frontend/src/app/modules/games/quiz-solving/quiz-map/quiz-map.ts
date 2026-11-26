import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { distanceKm } from '../../../../shared/map/path-length';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { repeatEvery } from '../../../../shared/position/repeat-every';
import { PageResult } from '../../../../shared/api/page-result';
import { formatKm, formatNumber } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  LocationChallengeDto,
  QuizMapItemDto,
  SocialChallengeMapItemDto,
  SocialChallengeRunDto,
} from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

export type ChallengeKind = 'quiz' | 'social' | 'location';
type KindFilter = ChallengeKind | 'all';

// One challenge of any kind, as the list and the map show it.
interface Challenge {
  kind: ChallengeKind;
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  solved: boolean;
  started: boolean;
  // The XP it gives, or for a solved one the XP earned.
  xpText: string;
}

const KIND_LABELS: Record<ChallengeKind, string> = {
  quiz: 'Kviz',
  social: 'Društveni',
  location: 'Lokacijski',
};

const KIND_MARKS: Record<ChallengeKind, string> = { quiz: '?', social: '☺', location: '⌖' };

// A location challenge is activated within this distance of its start
// (Games:LocationChallenges:ActivationDistanceInMeters on the server).
const LOCATION_ACTIVATION_METERS = 50;

// The tourist's challenges: quizzes, social and location challenges on one map, the list
// beside it ordered by distance. Choosing one opens its panel in place of the list.
@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-quiz-map',
  templateUrl: './quiz-map.html',
  styleUrl: './quiz-map.scss',
})
export class QuizMap {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly quizSolving = inject(QuizSolving);
  private readonly positionSimulation = inject(PositionSimulation);

  protected readonly user = this.auth.user;
  protected readonly isExplorer = computed(() => this.user()?.roles.includes('explorer') ?? false);

  protected readonly quizzes = httpResource<QuizMapItemDto[]>(() =>
    this.isExplorer() ? '/api/games/quizzes/map' : undefined,
  );
  protected readonly socialChallenges = httpResource<SocialChallengeMapItemDto[]>(() =>
    this.isExplorer() ? '/api/games/social-challenges/map' : undefined,
  );
  // null when the position is not set (the server answers 204).
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );

  protected readonly locationChallenges = httpResource<PageResult<LocationChallengeDto>>(() =>
    this.isExplorer() ? '/api/games/location-challenges?page=1&pageSize=100' : undefined,
  );

  protected readonly kindLabels = KIND_LABELS;
  protected readonly kindMarks = KIND_MARKS;
  protected readonly activationMeters = LOCATION_ACTIVATION_METERS;
  protected readonly formatNumber = formatNumber;

  protected readonly kind = signal<KindFilter>('all');
  protected readonly showSolved = signal(false);

  // Every challenge of the three kinds, nearest first once the position is known.
  private readonly all = computed<Challenge[]>(() => {
    const quizzes: Challenge[] = (this.quizzes.value() ?? []).map((quiz) => ({
      kind: 'quiz',
      id: quiz.id,
      name: quiz.name,
      latitude: quiz.latitude,
      longitude: quiz.longitude,
      solved: quiz.progress === 'Solved',
      started: quiz.progress === 'Started',
      xpText: quiz.earnedXp === null ? '' : formatNumber(quiz.earnedXp),
    }));
    const social: Challenge[] = (this.socialChallenges.value() ?? []).map((challenge) => ({
      kind: 'social',
      id: challenge.id,
      name: challenge.name,
      latitude: challenge.latitude,
      longitude: challenge.longitude,
      solved: challenge.solved,
      started: false,
      xpText: String(challenge.awardedXp ?? challenge.xpReward),
    }));
    const location: Challenge[] = (this.locationChallenges.value()?.items ?? []).map(
      (challenge) => ({
        kind: 'location',
        id: challenge.id,
        name: challenge.name,
        latitude: challenge.startLatitude,
        longitude: challenge.startLongitude,
        solved: false,
        started: false,
        xpText: String(challenge.xpReward),
      }),
    );
    const list = [...quizzes, ...social, ...location];
    const position = this.savedPosition();
    return position === null
      ? list
      : [...list].sort((a, b) => distanceKm(position, a) - distanceKm(position, b));
  });

  protected readonly solvedCount = computed(() => this.all().filter((item) => item.solved).length);

  protected readonly visible = computed(() =>
    this.all().filter(
      (item) =>
        (this.kind() === 'all' || item.kind === this.kind()) && (this.showSolved() || !item.solved),
    ),
  );

  protected readonly kindChips = computed(() =>
    (['all', 'quiz', 'social', 'location'] as KindFilter[]).map((kind) => ({
      kind,
      label: kind === 'all' ? 'Svi' : `${KIND_MARKS[kind]} ${KIND_LABELS[kind]}`,
    })),
  );

  // Reward first, so a solved quiz shows what it gave and the others what they give.
  protected readonly locations = computed<MapLocation[]>(() =>
    this.visible().map((item) => ({
      latitude: item.latitude,
      longitude: item.longitude,
      name: item.name,
      label: item.solved
        ? `✓ ${KIND_MARKS[item.kind]}`
        : item.kind === 'quiz'
          ? `? ${KIND_LABELS.quiz}`
          : `${KIND_MARKS[item.kind]} ${item.xpText} XP`,
      tone: item.solved ? 'owned' : item.kind,
    })),
  );

  // The challenge chosen from the list or the map; its panel replaces the list.
  protected readonly selectedKey = signal<string | null>(null);
  protected readonly selected = computed(
    () => this.all().find((item) => item.id === this.selectedKey()) ?? null,
  );
  protected readonly hovered = signal<string | null>(null);
  protected readonly activeIndex = computed(() => {
    const id = this.hovered() ?? this.selectedKey();
    const index = this.visible().findIndex((item) => item.id === id);
    return index === -1 ? null : index;
  });
  protected readonly focusIndex = computed(() => {
    const index = this.visible().findIndex((item) => item.id === this.selectedKey());
    return index === -1 ? null : index;
  });

  protected readonly selectedQuiz = computed(() => {
    const selected = this.selected();
    return selected?.kind === 'quiz'
      ? (this.quizzes.value()?.find((quiz) => quiz.id === selected.id) ?? null)
      : null;
  });
  protected readonly selectedLocation = computed(() => {
    const selected = this.selected();
    return selected?.kind === 'location'
      ? (this.locationChallenges.value()?.items.find((item) => item.id === selected.id) ?? null)
      : null;
  });

  // How far the tourist is from the chosen challenge, in a straight line.
  protected readonly distanceToSelected = computed(() => {
    const selected = this.selected();
    const position = this.savedPosition();
    return selected === null || position === null ? null : distanceKm(position, selected) * 1000;
  });
  protected readonly distanceText = computed(() => {
    const meters = this.distanceToSelected();
    return meters === null ? null : formatKm(meters / 1000);
  });
  protected readonly nearLocationStart = computed(() => {
    const meters = this.distanceToSelected();
    return meters !== null && meters <= LOCATION_ACTIVATION_METERS;
  });

  // A location challenge shows its activation circle and, while the tourist is outside it,
  // the walking route to its start.
  protected readonly area = computed(() => {
    const location = this.selectedLocation();
    return location === null
      ? null
      : {
          latitude: location.startLatitude,
          longitude: location.startLongitude,
          radiusMeters: LOCATION_ACTIVATION_METERS,
        };
  });
  protected readonly routeTarget = computed(() => {
    const selected = this.selected();
    if (selected === null || selected.solved || this.distanceToSelected() === null) {
      return null;
    }
    if (selected.kind === 'location' && this.nearLocationStart()) {
      return null;
    }
    return { latitude: selected.latitude, longitude: selected.longitude };
  });

  protected readonly selectedChallengeId = computed(() => {
    const selected = this.selected();
    return selected?.kind === 'social' ? selected.id : null;
  });
  protected readonly selectedChallenge = computed(() => {
    const id = this.selectedChallengeId();
    return id === null
      ? null
      : (this.socialChallenges.value()?.find((challenge) => challenge.id === id) ?? null);
  });

  // The player's run of the selected social challenge, reloaded after every command.
  protected readonly run = httpResource<SocialChallengeRunDto>(() => {
    const id = this.selectedChallengeId();
    return id === null ? undefined : `/api/games/social-challenges/${id}/run`;
  });

  protected readonly savedPosition = computed(() =>
    this.position.hasValue() ? this.position.value() : null,
  );
  protected readonly hasPosition = computed(() => this.savedPosition() !== null);

  protected readonly pending = signal(false);
  protected readonly positionError = signal<string | null>(null);
  protected readonly startError = signal<string | null>(null);
  protected readonly activateError = signal<string | null>(null);
  protected readonly reportError = signal<string | null>(null);

  // The social challenge whose location is sent every 10 seconds; null when none is checked.
  private readonly checkedChallengeId = signal<string | null>(null);
  // True once a check ended because the player is no longer in the run.
  private readonly droppedOut = signal(false);
  // True while a report is on its way; the timer does not wait for the previous request.
  private readonly reporting = signal(false);

  // The run as the server answered after the latest command, or null while it is loading,
  // so an older answer is never read as the outcome of the latest report.
  private readonly latestRun = computed(() =>
    this.run.status() === 'resolved' ? (this.run.value() ?? null) : null,
  );

  // Solved, once a report ended the run or the player had already solved it before.
  protected readonly solved = computed(() => {
    const run = this.run.hasValue() ? this.run.value() : null;
    return (this.selectedChallenge()?.solved ?? false) || (run?.completionMessage ?? null) !== null;
  });

  protected readonly awardedXp = computed(() => {
    const run = this.run.hasValue() ? this.run.value() : null;
    return run?.awardedXp ?? this.selectedChallenge()?.awardedXp ?? 0;
  });

  // From a click on "Aktiviraj" until the reloaded run shows the player out of it or solved.
  protected readonly checking = computed(() => {
    if (this.checkedChallengeId() === null) {
      return false;
    }
    const run = this.latestRun();
    return run === null || (run.isParticipant && run.completionMessage === null);
  });

  protected readonly showDroppedOut = computed(
    () =>
      this.droppedOut() ||
      (this.checkedChallengeId() !== null && !this.checking() && !this.solved()),
  );

  protected readonly progressPercent = computed(() => {
    const run = this.run.hasValue() ? this.run.value() : null;
    if (!run || run.requiredParticipants === 0) {
      return 0;
    }
    return Math.min(100, (run.activeParticipantCount / run.requiredParticipants) * 100);
  });

  // Every 10 seconds the page sends the last saved position for the checked social challenge,
  // without another click. It runs right away too, so it is declared after everything it reads.
  // It is never stopped: a new activation on the same visit only sets checkedChallengeId again.
  private readonly check = repeatEvery(10_000, () => this.reportSavedPosition());

  protected choosePin(index: number): void {
    const item = this.visible()[index];
    if (item !== undefined) {
      this.select(item.id);
    }
  }

  protected hoverPin(index: number | null): void {
    this.hovered.set(index === null ? null : (this.visible()[index]?.id ?? null));
  }

  // Choosing another challenge ends the check of the previous social one; the server stops
  // counting the player after 10 seconds.
  protected select(id: string | null): void {
    if (id === this.selectedKey()) {
      return;
    }
    this.selectedKey.set(id);
    this.startError.set(null);
    this.checkedChallengeId.set(null);
    this.droppedOut.set(false);
    this.activateError.set(null);
    this.reportError.set(null);
  }

  protected async setPosition(coordinates: MapCoordinates): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    this.positionError.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
    } catch (failure) {
      this.positionError.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async start(quiz: QuizMapItemDto): Promise<void> {
    const coordinates = this.position.value();
    if (!coordinates) return;
    this.pending.set(true);
    this.startError.set(null);
    try {
      await this.quizSolving.start(quiz.id, {
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
      });
      await this.router.navigate(['/games/quizzes', quiz.id, 'solve']);
    } catch (failure) {
      this.startError.set(serverMessage(failure, 'Kviz nije moguće započeti.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async activate(challengeId: string): Promise<void> {
    const coordinates = this.savedPosition();
    if (coordinates === null || this.pending()) return;
    this.pending.set(true);
    this.activateError.set(null);
    this.reportError.set(null);
    try {
      await this.quizSolving.activate(challengeId, {
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
      });
      this.droppedOut.set(false);
      this.checkedChallengeId.set(challengeId);
      this.run.reload();
    } catch (failure) {
      this.activateError.set(serverMessage(failure, 'Izazov nije moguće aktivirati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // One tick of the timer: nothing is sent until a challenge is activated and the position is known.
  private async reportSavedPosition(): Promise<void> {
    const challengeId = this.checkedChallengeId();
    if (challengeId === null || this.reporting()) return;
    // The run reloaded after the previous report says whether the check is over.
    if (!this.checking()) {
      this.droppedOut.set(!this.solved());
      this.checkedChallengeId.set(null);
      return;
    }
    const coordinates = this.savedPosition();
    if (coordinates === null) return;
    this.reporting.set(true);
    try {
      await this.quizSolving.reportLocation(challengeId, {
        latitude: coordinates.latitude,
        longitude: coordinates.longitude,
      });
      this.reportError.set(null);
    } catch (failure) {
      // Rejected also when the player is no longer in the run; the reloaded run then shows it.
      this.reportError.set(serverMessage(failure, 'Lokaciju nije moguće poslati.'));
    } finally {
      this.reporting.set(false);
      if (this.checkedChallengeId() === challengeId) {
        this.run.reload();
      }
    }
  }
}

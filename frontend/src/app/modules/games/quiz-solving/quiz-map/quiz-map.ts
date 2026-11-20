import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { repeatEvery } from '../../../../shared/position/repeat-every';
import { serverMessage } from '../../../../shared/util/server-message';
import {
  QuizMapItemDto,
  SocialChallengeMapItemDto,
  SocialChallengeRunDto,
} from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

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

  // The quiz pins first, in the same order as the list, then the social challenge pins. A pin's
  // index below the number of quizzes is the quiz's index; the rest are social challenges.
  protected readonly locations = computed<MapLocation[]>(() =>
    [...(this.quizzes.value() ?? []), ...(this.socialChallenges.value() ?? [])].map((item) => ({
      latitude: item.latitude,
      longitude: item.longitude,
      name: item.name,
    })),
  );

  protected readonly selectedIndex = signal<number | null>(null);
  protected readonly selectedQuiz = computed(() => {
    const index = this.selectedIndex();
    return index === null ? null : (this.quizzes.value()?.[index] ?? null);
  });

  protected readonly selectedChallengeId = signal<string | null>(null);
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
    () => this.droppedOut() || (this.checkedChallengeId() !== null && !this.checking() && !this.solved()),
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

  protected selectPin(index: number): void {
    const quizCount = this.quizzes.value()?.length ?? 0;
    if (index < quizCount) {
      this.selectQuiz(index);
      return;
    }
    const challenge = this.socialChallenges.value()?.[index - quizCount];
    if (challenge) {
      this.selectChallenge(challenge.id);
    }
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

  private selectQuiz(index: number): void {
    this.startError.set(null);
    this.selectedIndex.set(index);
    this.selectChallenge(null);
  }

  // The section shows one social challenge, so choosing another pin ends the check of the
  // previous one; the server stops counting the player after 10 seconds.
  private selectChallenge(id: string | null): void {
    if (id === this.selectedChallengeId()) return;
    if (id !== null) {
      this.selectedIndex.set(null);
    }
    this.selectedChallengeId.set(id);
    this.checkedChallengeId.set(null);
    this.droppedOut.set(false);
    this.activateError.set(null);
    this.reportError.set(null);
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

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import { PositionSimulation } from '../../../../shared/position/position-simulation';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizMapItemDto } from '../../api/games-api-types';
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
  // null when the position is not set (the server answers 204).
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );

  // The quizzes as map pins, in the same order as the list, so a pin's index is the quiz's index.
  protected readonly locations = computed<MapLocation[]>(() =>
    (this.quizzes.value() ?? []).map((quiz) => ({
      latitude: quiz.latitude,
      longitude: quiz.longitude,
      name: quiz.name,
    })),
  );

  protected readonly selectedIndex = signal<number | null>(null);
  protected readonly selectedQuiz = computed(() => {
    const index = this.selectedIndex();
    return index === null ? null : (this.quizzes.value()?.[index] ?? null);
  });

  protected readonly hasPosition = computed(
    () => this.position.hasValue() && this.position.value() !== null,
  );

  protected readonly pending = signal(false);
  protected readonly positionError = signal<string | null>(null);
  protected readonly startError = signal<string | null>(null);

  protected selectQuiz(index: number): void {
    this.startError.set(null);
    this.selectedIndex.set(index);
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
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeDto, QuizStatus } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';

const STATUSES: QuizStatus[] = ['Draft', 'Published', 'Retired', 'Archived'];

@Component({
  imports: [RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly quizAuthoring = inject(QuizAuthoring);

  protected readonly challenges = httpResource<ChallengeDto[]>(() => '/api/games/quizzes');

  // One section per state, in lifecycle order, each holding the challenges in that state.
  protected readonly sections = computed(() =>
    STATUSES.map((status) => ({
      status,
      challenges: (this.challenges.value() ?? []).filter(
        (challenge) => challenge.status === status,
      ),
    })),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async publish(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.publish(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije objavljen.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async retire(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.retire(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije povučen.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.archive(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije arhiviran.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.remove(id);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije obrisan.'));
    } finally {
      this.pending.set(false);
    }
  }
}

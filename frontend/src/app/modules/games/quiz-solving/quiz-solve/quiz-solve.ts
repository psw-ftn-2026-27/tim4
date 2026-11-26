import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { formatNumber } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizAttemptDto } from '../../api/games-api-types';
import { QuizSolving } from '../quiz-solving';

@Component({
  imports: [RouterLink],
  selector: 'app-quiz-solve',
  templateUrl: './quiz-solve.html',
  styleUrl: './quiz-solve.scss',
})
export class QuizSolve {
  readonly id = input.required<string>();
  private readonly auth = inject(Auth);
  private readonly quizSolving = inject(QuizSolving);

  protected readonly user = this.auth.user;
  protected readonly isExplorer = computed(() => this.user()?.roles.includes('explorer') ?? false);

  protected readonly attempt = httpResource<QuizAttemptDto>(() =>
    this.isExplorer() ? `/api/games/quizzes/${this.id()}/attempt` : undefined,
  );

  // The server answers 404 when the player has not started this quiz.
  protected readonly notStarted = computed(() => {
    const error = this.attempt.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  // Indices of the checked options of the current question.
  protected readonly selected = signal<number[]>([]);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // "37,5": one decimal at most, with a comma.
  protected readonly earnedXpText = computed(() => {
    const result = this.attempt.value()?.result;
    return result ? formatNumber(result.earnedXp) : '';
  });
  protected readonly percent = computed(() =>
    Math.round(this.attempt.value()?.result?.percentage ?? 0),
  );
  protected readonly fullScore = computed(() => {
    const result = this.attempt.value()?.result;
    return result !== null && result !== undefined && result.earnedXp >= result.totalXp;
  });

  // The number of the question whose answer was just saved, for the short notice.
  protected readonly saved = signal<number | null>(null);

  protected toggle(index: number): void {
    this.selected.update((selected) =>
      selected.includes(index)
        ? selected.filter((option) => option !== index)
        : [...selected, index],
    );
  }

  protected async answer(questionId: string): Promise<void> {
    if (this.selected().length === 0) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizSolving.answer(this.id(), questionId, { selectedOptions: this.selected() });
      this.saved.set((this.attempt.value()?.answeredCount ?? 0) + 1);
      this.selected.set([]);
      this.attempt.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Odgovor nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }
}

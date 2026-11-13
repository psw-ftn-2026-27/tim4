import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { form, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizContentDto, QuizDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';
import { QuizForm } from '../quiz-form/quiz-form';

@Component({
  imports: [QuizForm, RouterLink],
  selector: 'app-quiz-detail',
  styleUrl: './quiz-detail.scss',
  templateUrl: './quiz-detail.html',
})
export class QuizDetail {
  private readonly quizAuthoring = inject(QuizAuthoring);

  readonly id = input.required<string>();

  protected readonly quiz = httpResource<QuizDto>(() => `/api/games/quizzes/${this.id()}`);

  protected readonly notFound = computed(() => {
    const failure = this.quiz.error();
    return failure instanceof HttpErrorResponse && failure.status === 404;
  });

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(
    signal<QuizContentDto>({ name: '', instructions: '', questions: [] }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.instructions, { message: 'Uputstva su obavezna.' });
    },
  );

  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.form().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      questions: quiz.questions.map((question) => ({
        text: question.text,
        xpReward: question.xpReward,
        options: question.options.map((option) => ({
          text: option.text,
          isCorrect: option.isCorrect,
        })),
      })),
    });
    this.editing.set(true);
  }

  protected cancelEdit(): void {
    this.error.set(null);
    this.editing.set(false);
  }

  protected async save(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.edit(this.id(), this.form().value());
      this.quiz.reload();
      this.editing.set(false);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izmena kviza nije sačuvana.'));
    } finally {
      this.pending.set(false);
    }
  }
}

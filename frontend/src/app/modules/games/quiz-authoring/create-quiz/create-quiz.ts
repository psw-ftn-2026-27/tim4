import { Component, inject, signal } from '@angular/core';
import { form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizContentDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';
import { QuizForm } from '../quiz-form/quiz-form';

@Component({
  imports: [QuizForm, RouterLink],
  selector: 'app-create-quiz',
  styleUrl: './create-quiz.scss',
  templateUrl: './create-quiz.html',
})
export class CreateQuiz {
  private readonly quizAuthoring = inject(QuizAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal<QuizContentDto>({ name: '', instructions: '', questions: [] }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.instructions, { message: 'Uputstva su obavezna.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async save(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.create(this.form().value());
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }
}

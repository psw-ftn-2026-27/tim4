import { Component, inject, signal } from '@angular/core';
import { form, required, validate } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizContentDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';
import { QuizForm, QuizFormValue } from '../quiz-form/quiz-form';

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
    signal<QuizFormValue>({
      name: '',
      instructions: '',
      location: null,
      rangeInMeters: null,
      questions: [],
    }),
    (path) => {
      required(path.name, { message: 'Naziv je obavezan.' });
      required(path.instructions, { message: 'Uputstva su obavezna.' });
      required(path.location, { message: 'Izaberi lokaciju na mapi' });
      required(path.rangeInMeters, { message: 'Opseg aktivacije je obavezan.' });
      validate(path.rangeInMeters, ({ value }) => {
        const range = value();
        return range === null || (Number.isInteger(range) && range > 0)
          ? null
          : { kind: 'positiveInteger', message: 'Opseg aktivacije mora biti pozitivan ceo broj.' };
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async save(quiz: QuizContentDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.create(quiz);
      await this.router.navigate(['/games/challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije sačuvan.'));
    } finally {
      this.pending.set(false);
    }
  }
}

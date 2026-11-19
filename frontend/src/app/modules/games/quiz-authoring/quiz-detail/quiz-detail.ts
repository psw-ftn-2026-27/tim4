import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { form, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';
import { serverMessage } from '../../../../shared/util/server-message';
import { QuizContentDto, QuizDto } from '../../api/games-api-types';
import { QuizAuthoring } from '../quiz-authoring';
import { QuizForm, QuizFormValue } from '../quiz-form/quiz-form';

@Component({
  imports: [LocationMap, QuizForm, RouterLink],
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

  // The quiz location as the one pin of the view map.
  protected readonly quizLocation = computed<MapLocation[]>(() => {
    const quiz = this.quiz.value();
    return quiz ? [{ latitude: quiz.latitude, longitude: quiz.longitude, name: quiz.name }] : [];
  });

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

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

  protected startEdit(quiz: QuizDto): void {
    this.error.set(null);
    this.form().reset({
      name: quiz.name,
      instructions: quiz.instructions,
      location: { latitude: quiz.latitude, longitude: quiz.longitude },
      rangeInMeters: quiz.rangeInMeters,
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

  protected async save(quiz: QuizContentDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.quizAuthoring.edit(this.id(), quiz);
      this.quiz.reload();
      this.editing.set(false);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Izmena kviza nije sačuvana.'));
    } finally {
      this.pending.set(false);
    }
  }
}

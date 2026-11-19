import { Component, input, output } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { LocationMap, MapCoordinates } from '../../../../shared/map/location-map';
import { QuestionContentDto, QuizContentDto } from '../../api/games-api-types';

// The form's model: the location is one map pin (null until the map is clicked) and the
// range is null while its field is empty. On submit they become the DTO's flat fields.
export interface QuizFormValue {
  name: string;
  instructions: string;
  location: MapCoordinates | null;
  rangeInMeters: number | null;
  questions: QuestionContentDto[];
}

// The page owns the form() so it can fill it in a click handler; this component only
// shows the fields and adds or removes questions and options in the page's form.
@Component({
  imports: [FormField, LocationMap],
  selector: 'app-quiz-form',
  styleUrl: './quiz-form.scss',
  templateUrl: './quiz-form.html',
})
export class QuizForm {
  readonly quizForm = input.required<FieldTree<QuizFormValue>>();
  readonly pending = input(false);
  readonly error = input<string | null>(null);

  readonly save = output<QuizContentDto>();

  protected submit(event: Event): void {
    event.preventDefault();
    const { name, instructions, location, rangeInMeters, questions } = this.quizForm()().value();
    if (!this.quizForm()().valid() || location === null || rangeInMeters === null) {
      this.quizForm()().markAsTouched();
      return;
    }
    this.save.emit({
      name,
      instructions,
      questions,
      latitude: location.latitude,
      longitude: location.longitude,
      rangeInMeters,
    });
  }

  protected addQuestion(): void {
    const newQuestion: QuestionContentDto = {
      text: '',
      xpReward: 10,
      options: [
        { text: '', isCorrect: false },
        { text: '', isCorrect: false },
      ],
    };
    this.quizForm()
      .questions()
      .value.update((questions) => [...questions, newQuestion]);
  }

  protected removeQuestion(questionIndex: number): void {
    this.quizForm()
      .questions()
      .value.update((questions) => questions.filter((_, index) => index !== questionIndex));
  }

  protected addOption(questionIndex: number): void {
    this.quizForm()
      .questions[questionIndex].options()
      .value.update((options) => [...options, { text: '', isCorrect: false }]);
  }

  protected removeOption(questionIndex: number, optionIndex: number): void {
    this.quizForm()
      .questions[questionIndex].options()
      .value.update((options) => options.filter((_, index) => index !== optionIndex));
  }
}

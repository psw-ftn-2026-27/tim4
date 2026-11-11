import { Component, input, output } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { QuestionContentDto, QuizContentDto } from '../../api/games-api-types';

// The page owns the form() so it can fill it in a click handler; this component only
// shows the fields and adds or removes questions and options in the page's form.
@Component({
  imports: [FormField],
  selector: 'app-quiz-form',
  styleUrl: './quiz-form.scss',
  templateUrl: './quiz-form.html',
})
export class QuizForm {
  readonly quizForm = input.required<FieldTree<QuizContentDto>>();
  readonly pending = input(false);
  readonly error = input<string | null>(null);

  readonly save = output<void>();

  protected submit(event: Event): void {
    event.preventDefault();
    this.save.emit();
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

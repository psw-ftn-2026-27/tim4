import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { playChallenges, questionScore, quizQuestions, xpText } from '../mock';

type Start = 'new' | 'resume' | 'done';

// PROTOTYPE of scenario S4, solving a quiz. One question at a time in a centred column, as on
// the server: answered in order, once, with no way back. The two rules that surprise a tester,
// "more than one option may be correct" and "a wrong tick costs points", are said next to the
// options, not only before the start. The result says what was earned and why it may be less.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-quiz',
  styleUrl: './tourist-quiz.scss',
  templateUrl: './tourist-quiz.html',
})
export class TouristQuiz {
  protected readonly quiz = playChallenges.find((challenge) => challenge.id === 'q1')!;
  protected readonly questions = quizQuestions;
  protected readonly total = quizQuestions.length;
  protected readonly maxXp = quizQuestions.reduce((sum, question) => sum + question.xp, 0);

  protected readonly start = signal<Start>('new');
  // The XP of every answered question, in order; its length is the number answered.
  protected readonly scores = signal<number[]>([]);
  protected readonly selected = signal<number[]>([]);
  // The number of the question whose answer was just saved, for the short notice.
  protected readonly saved = signal<number | null>(null);

  protected readonly answered = computed(() => this.scores().length);
  protected readonly finished = computed(() => this.answered() === this.total);
  protected readonly question = computed(() =>
    this.finished() ? null : this.questions[this.answered()],
  );
  protected readonly isLast = computed(() => this.answered() === this.total - 1);
  protected readonly progress = computed(() => `${(this.answered() / this.total) * 100}%`);

  protected readonly earned = computed(() => this.scores().reduce((sum, xp) => sum + xp, 0));
  protected readonly earnedText = computed(() => xpText(this.earned()));
  protected readonly percent = computed(() => Math.round((this.earned() / this.maxXp) * 100));

  protected readonly xpText = xpText;

  protected setStart(start: Start): void {
    this.start.set(start);
    this.selected.set([]);
    this.saved.set(null);
    if (start === 'new') {
      this.scores.set([]);
    } else if (start === 'resume') {
      this.scores.set([20, 15]);
    } else {
      this.scores.set([20, 15, 25, 0, 18.75]);
    }
  }

  protected toggle(option: number): void {
    this.selected.update((selected) =>
      selected.includes(option)
        ? selected.filter((item) => item !== option)
        : [...selected, option].sort(),
    );
  }

  protected answer(): void {
    const question = this.question();
    if (question === null || this.selected().length === 0) {
      return;
    }
    this.saved.set(this.answered() + 1);
    this.scores.update((scores) => [...scores, questionScore(question, this.selected())]);
    this.selected.set([]);
  }
}

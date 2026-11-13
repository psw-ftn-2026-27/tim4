import { httpResource } from '@angular/common/http';
import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ChallengeDto, QuizStatus } from '../../api/games-api-types';

const STATUSES: QuizStatus[] = ['Draft', 'Published', 'Retired', 'Archived'];

@Component({
  imports: [RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
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
}

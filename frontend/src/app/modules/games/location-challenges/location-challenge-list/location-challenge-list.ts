import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationChallengeDto } from '../../api/games-api-types';
import { AdminTabs } from '../../quiz-authoring/admin-tabs/admin-tabs';

// The administrator's location challenges. Tourists find them on the challenge map; here the
// administrator sees the list and opens one to check its hidden places. Location challenges
// have no lifecycle on the server yet, so there is no state and nothing to publish.
@Component({
  imports: [AdminTabs, RouterLink],
  selector: 'app-location-challenge-list',
  styleUrl: './location-challenge-list.scss',
  templateUrl: './location-challenge-list.html',
})
export class LocationChallengeList {
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly challenges = httpResource<PageResult<LocationChallengeDto>>(
    () => `/api/games/location-challenges?page=${this.page()}&pageSize=${this.pageSize}`,
  );
  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.challenges.value()?.totalCount ?? 0) / this.pageSize)),
  );
}

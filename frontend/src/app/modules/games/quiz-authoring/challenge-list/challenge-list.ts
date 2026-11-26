import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { ChallengeDto, QuizStatus } from '../../api/games-api-types';
import { AdminTabs } from '../admin-tabs/admin-tabs';
import { QuizAuthoring } from '../quiz-authoring';

type Filter = QuizStatus | 'all';

export const STATUS_LABELS: Record<QuizStatus, { label: string; badge: string }> = {
  Draft: { label: '✎ Nacrt', badge: 'badge badge--draft' },
  Published: { label: '● Objavljen', badge: 'badge badge--published' },
  Retired: { label: '⊘ Povučen', badge: 'badge badge--retired' },
  Archived: { label: '▣ Arhiviran', badge: 'badge badge--archived' },
};

// The administrator's quizzes in one table: a chip filters by state, each row has the one
// next step of its lifecycle, and deleting asks in the row.
@Component({
  imports: [AdminTabs, RouterLink],
  selector: 'app-challenge-list',
  styleUrl: './challenge-list.scss',
  templateUrl: './challenge-list.html',
})
export class ChallengeList {
  private readonly quizAuthoring = inject(QuizAuthoring);

  protected readonly challenges = httpResource<ChallengeDto[]>(() => '/api/games/quizzes');

  protected readonly statusLabels = STATUS_LABELS;
  protected readonly filter = signal<Filter>('all');
  protected readonly filters = computed(() => {
    const all = this.challenges.value() ?? [];
    const count = (status: QuizStatus) => all.filter((item) => item.status === status).length;
    return [
      { value: 'all' as Filter, label: 'Svi', count: all.length },
      ...(['Draft', 'Published', 'Retired', 'Archived'] as QuizStatus[]).map((status) => ({
        value: status as Filter,
        label: STATUS_LABELS[status].label,
        count: count(status),
      })),
    ];
  });
  protected readonly visible = computed(() =>
    (this.challenges.value() ?? []).filter(
      (item) => this.filter() === 'all' || item.status === this.filter(),
    ),
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  // The quiz whose deletion waits for the administrator's confirmation.
  protected readonly deleting = signal<string | null>(null);

  protected async publish(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.quizAuthoring.publish(id);
      this.notice.set('Kviz je objavljen: turisti ga vide na mapi.');
      this.deleting.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije objavljen.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async retire(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.quizAuthoring.retire(id);
      this.notice.set('Kviz je povučen sa mape.');
      this.deleting.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije povučen.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async archive(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.quizAuthoring.archive(id);
      this.notice.set('Kviz je arhiviran.');
      this.deleting.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije arhiviran.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(id: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.quizAuthoring.remove(id);
      this.notice.set('Kviz je obrisan.');
      this.deleting.set(null);
      this.challenges.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kviz nije obrisan.'));
    } finally {
      this.pending.set(false);
    }
  }
}

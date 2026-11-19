import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengePreparation } from '../social-challenge-preparation';

@Component({
  imports: [RouterLink],
  selector: 'app-social-challenge-list',
  templateUrl: './social-challenge-list.html',
  styleUrl: './social-challenge-list.scss',
})
export class SocialChallengeList {
  private readonly auth = inject(Auth);
  private readonly preparation = inject(SocialChallengePreparation);

  protected readonly isAdministrator = computed(() =>
    this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly page = signal(1);
  protected readonly pageSize = 20;
  protected readonly challenges = httpResource<PageResult<SocialChallengeDto>>(() =>
    this.isAdministrator()
      ? `/api/games/social-challenges?page=${this.page()}&pageSize=${this.pageSize}`
      : undefined,
  );
  protected readonly selectedId = signal<string | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly stateErrorId = signal<string | null>(null);

  protected async changeState(challenge: SocialChallengeDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.stateErrorId.set(null);
    try {
      switch (challenge.status) {
        case 'Draft':
          await this.preparation.publish(challenge.id);
          break;
        case 'Published':
          await this.preparation.retire(challenge.id);
          break;
        case 'Retired':
          await this.preparation.archive(challenge.id);
          break;
      }
      this.challenges.reload();
    } catch (failure) {
      this.stateErrorId.set(challenge.id);
      this.error.set(serverMessage(failure, 'Promena stanja izazova nije uspela.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected changePage(page: number): void {
    this.cancelDelete();
    this.page.set(page);
  }

  protected requestDelete(id: string): void {
    this.error.set(null);
    this.stateErrorId.set(null);
    this.selectedId.set(id);
  }

  protected cancelDelete(): void {
    this.selectedId.set(null);
    this.error.set(null);
  }

  protected async deleteChallenge(id: string): Promise<void> {
    const wasOnlyChallengeOnPage = this.challenges.value()?.items.length === 1;
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.preparation.remove(id);
      this.cancelDelete();
      if (wasOnlyChallengeOnPage && this.page() > 1) {
        this.page.update((page) => page - 1);
      } else {
        this.challenges.reload();
      }
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Brisanje izazova nije uspelo.'));
    } finally {
      this.pending.set(false);
    }
  }
}

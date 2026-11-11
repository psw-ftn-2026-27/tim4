import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { SocialChallengeDetailsDto, SocialChallengeDto } from '../../api/games-api-types';
import { SocialChallengeForm } from '../social-challenge-form/social-challenge-form';
import { SocialChallengePreparation } from '../social-challenge-preparation';

@Component({
  imports: [RouterLink, SocialChallengeForm],
  selector: 'app-social-challenge-editor',
  templateUrl: './social-challenge-editor.html',
  styleUrl: './social-challenge-editor.scss',
})
export class SocialChallengeEditor {
  readonly id = input<string>();
  private readonly auth = inject(Auth);
  private readonly preparation = inject(SocialChallengePreparation);
  private readonly router = inject(Router);

  protected readonly isAdministrator = computed(() =>
    this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly challenge = httpResource<SocialChallengeDto>(() => {
    const id = this.id();
    return this.isAdministrator() && id ? `/api/games/social-challenges/${id}` : undefined;
  });
  protected readonly loadError = computed(() => {
    const error = this.challenge.error();
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'Društveni izazov ne postoji.';
    }
    return 'Učitavanje izazova nije uspelo.';
  });
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async save(details: SocialChallengeDetailsDto): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      const id = this.id();
      if (id) {
        await this.preparation.update(id, details);
      } else {
        await this.preparation.create(details);
      }
      await this.router.navigate(['/games/social-challenges']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Čuvanje izazova nije uspelo.'));
    } finally {
      this.pending.set(false);
    }
  }
}

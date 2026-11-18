import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import {
  FormField,
  email,
  form,
  max,
  maxLength,
  min,
  required,
  validate,
} from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { GiftCardStatus, MyGiftCardsDto, WalletBalanceDto } from '../../api/payment-api-types';
import { PersonalGiftCards } from '../personal-gift-cards';

interface GiftCardForm {
  recipientEmail: string;
  amount: number | null;
  message: string;
}

const EMPTY_GIFT_CARD: GiftCardForm = { recipientEmail: '', amount: null, message: '' };
const MIN_AMOUNT = 10;
const MAX_AMOUNT = 1000;
const MAX_MESSAGE_LENGTH = 200;

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-gift-cards',
  styleUrl: './my-gift-cards.scss',
  templateUrl: './my-gift-cards.html',
})
export class MyGiftCards {
  private readonly personalGiftCards = inject(PersonalGiftCards);
  protected readonly auth = inject(Auth);

  protected readonly maxMessageLength = MAX_MESSAGE_LENGTH;
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly giftCards = httpResource<MyGiftCardsDto>(() =>
    this.isExplorer() ? '/api/payment/gift-cards' : undefined,
  );
  protected readonly balance = httpResource<WalletBalanceDto>(() =>
    this.isExplorer() ? '/api/payment/wallet/balance' : undefined,
  );

  protected readonly giftCardForm = form(signal<GiftCardForm>({ ...EMPTY_GIFT_CARD }), (path) => {
    required(path.recipientEmail, { message: 'Unesite korisničko ime (mejl) primaoca.' });
    email(path.recipientEmail, { message: 'Korisničko ime je mejl adresa.' });
    validate(path.recipientEmail, ({ value }) =>
      value().trim().toLowerCase() === this.auth.user()?.email.toLowerCase()
        ? { kind: 'self', message: 'Ne možete da poklonite karticu sebi.' }
        : undefined,
    );
    required(path.amount, { message: 'Unesite iznos u EC.' });
    min(path.amount, MIN_AMOUNT, { message: `Iznos mora biti najmanje ${MIN_AMOUNT} EC.` });
    max(path.amount, MAX_AMOUNT, { message: `Iznos može biti najviše ${MAX_AMOUNT} EC.` });
    validate(path.amount, ({ value }) =>
      value() !== null && !Number.isInteger(value())
        ? { kind: 'integer', message: 'Iznos mora biti ceo broj EC.' }
        : undefined,
    );
    validate(path.message, ({ value }) =>
      value().trim() === '' ? { kind: 'required', message: 'Unesite poruku.' } : undefined,
    );
    maxLength(path.message, MAX_MESSAGE_LENGTH, {
      message: `Poruka može imati najviše ${MAX_MESSAGE_LENGTH} karaktera.`,
    });
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);

  // Gift card dates follow UTC on the server, so the date part of an ISO value is shown as it is.
  protected formatDate(value: string): string {
    const [year, month, day] = value.slice(0, 10).split('-');
    return `${day}.${month}.${year}.`;
  }

  protected statusLabel(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return '● Važeća';
      case 'Redeemed':
        return '✓ Iskorišćena';
      default:
        return '⊘ Istekla';
    }
  }

  protected statusClass(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return 'badge badge--published';
      case 'Redeemed':
        return 'badge badge--muted';
      default:
        return 'badge badge--retired';
    }
  }

  protected reload(): void {
    this.giftCards.reload();
    this.balance.reload();
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.pending()) return;
    if (!this.giftCardForm().valid()) {
      this.giftCardForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      const recipientEmail = this.giftCardForm.recipientEmail().value().trim();
      const amount = this.giftCardForm.amount().value() ?? 0;
      await this.personalGiftCards.buyGiftCard({
        recipientEmail,
        amount,
        message: this.giftCardForm.message().value().trim(),
      });
      this.giftCardForm().reset({ ...EMPTY_GIFT_CARD });
      this.success.set(`Poklon kartica od ${amount} EC je poslata korisniku ${recipientEmail}.`);
      this.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kartica nije napravljena. Pokušajte ponovo.'));
      this.balance.reload();
    } finally {
      this.pending.set(false);
    }
  }
}

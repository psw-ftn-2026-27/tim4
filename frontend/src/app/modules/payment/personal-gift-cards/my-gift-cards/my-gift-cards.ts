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
import { PageResult } from '../../../../shared/api/page-result';
import { GiftCardDto, WalletBalanceDto } from '../../api/payment-api-types';
import { GiftCardList } from '../gift-card-list/gift-card-list';
import { PersonalGiftCards } from '../personal-gift-cards';

function pageCount(result: PageResult<GiftCardDto> | undefined): number {
  return Math.max(1, Math.ceil((result?.totalCount ?? 0) / PAGE_SIZE));
}

interface GiftCardForm {
  recipientEmail: string;
  amount: number | null;
  message: string;
}

const EMPTY_GIFT_CARD: GiftCardForm = { recipientEmail: '', amount: null, message: '' };
const MIN_AMOUNT = 10;
const MAX_AMOUNT = 1000;
const MAX_MESSAGE_LENGTH = 200;
const PAGE_SIZE = 10;

@Component({
  imports: [FormField, GiftCardList, RouterLink],
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
  protected readonly boughtPage = signal(1);
  protected readonly receivedPage = signal(1);
  protected readonly bought = httpResource<PageResult<GiftCardDto>>(() =>
    this.isExplorer()
      ? `/api/payment/gift-cards/bought?page=${this.boughtPage()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );
  protected readonly received = httpResource<PageResult<GiftCardDto>>(() =>
    this.isExplorer()
      ? `/api/payment/gift-cards/received?page=${this.receivedPage()}&pageSize=${PAGE_SIZE}`
      : undefined,
  );
  protected readonly boughtPageCount = computed(() => pageCount(this.bought.value()));
  protected readonly receivedPageCount = computed(() => pageCount(this.received.value()));
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

  protected changeBoughtPage(step: number): void {
    this.boughtPage.update((page) => Math.min(Math.max(page + step, 1), this.boughtPageCount()));
  }

  protected changeReceivedPage(step: number): void {
    this.receivedPage.update((page) =>
      Math.min(Math.max(page + step, 1), this.receivedPageCount()),
    );
  }

  protected reload(): void {
    this.bought.reload();
    this.received.reload();
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
      this.balance.reload();
      // The new card is first on page 1 of the bought cards; changing the page reloads the list by itself.
      if (this.boughtPage() === 1) {
        this.bought.reload();
      } else {
        this.boughtPage.set(1);
      }
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kartica nije napravljena. Pokušajte ponovo.'));
      this.balance.reload();
    } finally {
      this.pending.set(false);
    }
  }
}

import { Component, computed, signal } from '@angular/core';
import { FormField, form, max, maxLength, min, required, email } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import {
  GiftCardStatus,
  ProtoGiftCard,
  giftCardRules,
  receivedCards,
  sentCards,
  walletBalance,
} from '../mock';

type Tab = 'received' | 'sent';

const STATUS: Record<GiftCardStatus, { label: string; badge: string }> = {
  Valid: { label: '● Važi', badge: 'badge badge--published' },
  Redeemed: { label: '✓ Iskorišćena', badge: 'badge badge--muted' },
  Expired: { label: '⊘ Istekla', badge: 'badge badge--retired' },
};

// PROTOTYPE of scenario S5, gift cards. Received cards look like cards and say how to use them;
// giving one is a form next to a live preview of what the friend will see, with the wallet
// balance before and after, so the tourist knows the EC leave the wallet now.
@Component({
  imports: [FormField, RouterLink],
  selector: 'app-tourist-gift-cards',
  styleUrl: './tourist-gift-cards.scss',
  templateUrl: './tourist-gift-cards.html',
})
export class TouristGiftCards {
  protected readonly status = STATUS;
  protected readonly rules = giftCardRules;
  protected readonly quickAmounts = [20, 50, 100];

  protected readonly tab = signal<Tab>('received');
  protected readonly giving = signal(false);
  protected readonly notice = signal<string | null>(null);
  protected readonly balance = signal(walletBalance);
  protected readonly sent = signal<ProtoGiftCard[]>([...sentCards]);
  protected readonly received = receivedCards;

  protected readonly giftForm = form(
    signal({ recipient: '', amount: 50 as number | null, message: '' }),
    (path) => {
      required(path.recipient, { message: 'Unesite mejl primaoca.' });
      email(path.recipient, { message: 'Mejl nije ispravan.' });
      required(path.amount, { message: 'Unesite iznos.' });
      min(path.amount, giftCardRules.min, { message: `Najmanje ${giftCardRules.min} EC.` });
      max(path.amount, giftCardRules.max, { message: `Najviše ${giftCardRules.max} EC.` });
      required(path.message, { message: 'Napišite kratku poruku.' });
      maxLength(path.message, giftCardRules.maxMessage, {
        message: `Najviše ${giftCardRules.maxMessage} znakova.`,
      });
    },
  );

  protected readonly amount = computed(() => {
    const value = this.giftForm.amount().value();
    return value === null || Number.isNaN(value) ? 0 : value;
  });
  protected readonly after = computed(() => this.balance() - this.amount());

  protected readonly tabs = computed(() => [
    { value: 'received' as Tab, label: 'Dobijene', count: this.received.length },
    { value: 'sent' as Tab, label: 'Poklonjene', count: this.sent().length },
  ]);

  protected setAmount(value: number): void {
    this.giftForm.amount().value.set(value);
  }

  protected give(event: Event): void {
    event.preventDefault();
    if (!this.giftForm().valid() || this.after() < 0) {
      this.giftForm().markAsTouched();
      return;
    }
    const value = this.giftForm().value();
    this.sent.update((list) => [
      {
        id: 'new' + list.length,
        person: value.recipient,
        amount: this.amount(),
        message: value.message,
        createdAt: '21.11.2026.',
        validUntil: '21.11.2029.',
        status: 'Valid',
      },
      ...list,
    ]);
    this.balance.update((balance) => balance - this.amount());
    this.notice.set(
      `Kartica od ${this.amount()} EC je poslata na ${value.recipient}. Videće je u svojim poklon karticama.`,
    );
    this.giving.set(false);
    this.tab.set('sent');
    this.giftForm().reset({ recipient: '', amount: 50, message: '' });
  }
}

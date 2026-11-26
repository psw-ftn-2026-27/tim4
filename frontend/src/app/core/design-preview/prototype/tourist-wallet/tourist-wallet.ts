import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ProtoTransaction,
  TransactionKind,
  receivedCards,
  transactions,
  walletBalance,
} from '../mock';

type Filter = 'all' | 'in' | 'out';

const KINDS: Record<TransactionKind, { label: string; mark: string }> = {
  Credit: { label: 'Uplata', mark: '+' },
  Purchase: { label: 'Kupovina ture', mark: '−' },
  GiftCard: { label: 'Poklon kartica', mark: '🎁' },
};

// PROTOTYPE of scenario S5, the wallet. On the left what the tourist has (EC and gift cards) and
// where EC come from; on the right what happened, by day, like a bank statement. A new tourist
// with no EC gets an explanation instead of "0 EC" and an empty table.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-wallet',
  styleUrl: './tourist-wallet.scss',
  templateUrl: './tourist-wallet.html',
})
export class TouristWallet {
  protected readonly kinds = KINDS;
  // Prototype switch: the wallet of a tourist who has never received EC.
  protected readonly empty = signal(false);
  protected readonly filter = signal<Filter>('all');

  protected readonly balance = computed(() => (this.empty() ? 0 : walletBalance));
  protected readonly validCards = computed(() =>
    this.empty() ? [] : receivedCards.filter((card) => card.status === 'Valid'),
  );
  protected readonly cardsTotal = computed(() =>
    this.validCards().reduce((sum, card) => sum + card.amount, 0),
  );

  // Transactions grouped under their date, newest first.
  protected readonly days = computed(() => {
    const list = this.empty()
      ? []
      : transactions.filter(
          (t) =>
            this.filter() === 'all' ||
            (this.filter() === 'in' ? t.kind === 'Credit' : t.kind !== 'Credit'),
        );
    const days: { date: string; items: ProtoTransaction[] }[] = [];
    for (const t of list) {
      const last = days[days.length - 1];
      if (last?.date === t.date) {
        last.items.push(t);
      } else {
        days.push({ date: t.date, items: [t] });
      }
    }
    return days;
  });

  protected readonly filters: { value: Filter; label: string }[] = [
    { value: 'all', label: 'Sve' },
    { value: 'in', label: 'Uplate' },
    { value: 'out', label: 'Potrošeno' },
  ];
}

import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatDate, formatDateTime } from '../../../../shared/util/format';
import { TransactionKind, WalletTransactionDto } from '../../api/payment-api-types';

const KINDS: Record<TransactionKind, { label: string; mark: string }> = {
  Credit: { label: 'Uplata od administratora', mark: '+' },
  Purchase: { label: 'Kupovina', mark: '−' },
  GiftCard: { label: 'Poklon kartica', mark: '🎁' },
};

// The wallet's changes by day, like a bank statement: what it was, when, and how much.
@Component({
  imports: [RouterLink],
  selector: 'app-wallet-transactions',
  styleUrl: './wallet-transactions.scss',
  templateUrl: './wallet-transactions.html',
})
export class WalletTransactions {
  // Newest first, as the server sends them.
  readonly transactions = input.required<WalletTransactionDto[]>();

  protected readonly kinds = KINDS;

  // The transactions under their date.
  protected readonly days = computed(() => {
    const days: { date: string; items: WalletTransactionDto[] }[] = [];
    for (const transaction of this.transactions()) {
      const date = formatDate(transaction.submittedAt);
      const last = days[days.length - 1];
      if (last?.date === date) {
        last.items.push(transaction);
      } else {
        days.push({ date, items: [transaction] });
      }
    }
    return days;
  });

  // Only the time: the date is the heading of the day.
  protected time(value: string): string {
    return formatDateTime(value).split(' ')[1];
  }
}

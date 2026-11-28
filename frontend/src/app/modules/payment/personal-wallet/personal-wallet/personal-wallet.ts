import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { GiftCardDto, WalletBalanceDto, WalletTransactionDto } from '../../api/payment-api-types';
import { WalletTransactions } from '../wallet-transactions/wallet-transactions';

type Filter = 'all' | 'in' | 'out';

const PAGE_SIZE = 20;

// The wallet: on the left what the tourist has (EC and gift cards) and where EC come from; on
// the right what happened, by day, like a bank statement. A tourist with no EC yet gets an
// explanation instead of "0 EC" and an empty table.
@Component({
  imports: [WalletTransactions, RouterLink],
  selector: 'app-personal-wallet',
  styleUrl: './personal-wallet.scss',
  templateUrl: './personal-wallet.html',
})
export class PersonalWallet {
  protected readonly page = signal(1);
  protected readonly filter = signal<Filter>('all');
  protected readonly filters: { value: Filter; label: string }[] = [
    { value: 'all', label: 'Sve' },
    { value: 'in', label: 'Uplate' },
    { value: 'out', label: 'Potrošeno' },
  ];

  protected readonly balance = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');
  protected readonly transactions = httpResource<PageResult<WalletTransactionDto>>(
    () => `/api/payment/wallet/transactions?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );
  // The received gift cards, for the tile that says what can still be used at checkout.
  protected readonly receivedCards = httpResource<PageResult<GiftCardDto>>(
    () => '/api/payment/gift-cards/received?page=1&pageSize=50',
  );

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.transactions.value()?.totalCount ?? 0) / PAGE_SIZE)),
  );

  protected readonly validCards = computed(() =>
    (this.receivedCards.value()?.items ?? []).filter((card) => card.status === 'Valid'),
  );
  protected readonly cardsTotal = computed(() =>
    this.validCards().reduce((sum, card) => sum + card.amount, 0),
  );

  // The page's transactions the filter chips allow.
  protected readonly shown = computed(() => {
    const filter = this.filter();
    return (this.transactions.value()?.items ?? []).filter(
      (transaction) => filter === 'all' || (filter === 'in') === (transaction.kind === 'Credit'),
    );
  });

  protected changePage(change: number): void {
    const page = this.page() + change;
    if (page >= 1 && page <= this.pageCount()) {
      this.page.set(page);
    }
  }

  protected retry(): void {
    this.balance.reload();
    this.transactions.reload();
  }
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { WalletBalanceDto, WalletTransactionDto } from '../../api/payment-api-types';
import { WalletTransactions } from '../wallet-transactions/wallet-transactions';

@Component({
  imports: [WalletTransactions, RouterLink],
  selector: 'app-personal-wallet',
  styleUrl: './personal-wallet.scss',
  templateUrl: './personal-wallet.html',
})
export class PersonalWallet {
  protected readonly auth = inject(Auth);
  protected readonly page = signal(1);
  protected readonly pageSize = 20;

  protected readonly balance = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');
  protected readonly transactions = httpResource<PageResult<WalletTransactionDto>>(
    () => `/api/payment/wallet/transactions?page=${this.page()}&pageSize=${this.pageSize}`,
  );
  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.transactions.value()?.totalCount ?? 0) / this.pageSize)),
  );

  protected previousPage(): void {
    if (this.page() > 1) {
      this.page.update((page) => page - 1);
    }
  }

  protected nextPage(): void {
    if (this.page() < this.pageCount()) {
      this.page.update((page) => page + 1);
    }
  }

  protected retry(): void {
    this.balance.reload();
    this.transactions.reload();
  }
}

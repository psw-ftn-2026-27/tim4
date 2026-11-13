import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ExplorerWalletDto, WalletTransactionDto } from '../../api/payment-api-types';
import { WalletTransactions } from '../../personal-wallet/wallet-transactions/wallet-transactions';
import { WalletAdministrationService } from '../wallet-administration';

@Component({
  imports: [FormField, RouterLink, WalletTransactions],
  selector: 'app-wallet-administration',
  styleUrl: './wallet-administration.scss',
  templateUrl: './wallet-administration.html',
})
export class WalletAdministration {
  private readonly auth = inject(Auth);
  private readonly commands = inject(WalletAdministrationService);

  protected readonly isAdministrator = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
  protected readonly page = signal(1);
  protected readonly historyPage = signal(1);
  protected readonly pageSize = 20;
  protected readonly selectedUser = signal<ExplorerWalletDto | null>(null);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly paymentForm = form(signal({ amount: 1 }), (path) => {
    required(path.amount, { message: 'Unesi iznos.' });
    min(path.amount, 1, { message: 'Iznos mora biti pozitivan ceo broj EC.' });
    max(path.amount, 2147483647, { message: 'Iznos je prevelik.' });
  });

  protected readonly users = httpResource<PageResult<ExplorerWalletDto>>(() =>
    this.isAdministrator()
      ? `/api/payment/wallet/users?page=${this.page()}&pageSize=${this.pageSize}`
      : undefined,
  );
  protected readonly transactions = httpResource<PageResult<WalletTransactionDto>>(() => {
    const user = this.selectedUser();
    return this.isAdministrator() && user
      ? `/api/payment/wallet/users/${user.userId}/transactions?page=${this.historyPage()}&pageSize=${this.pageSize}`
      : undefined;
  });
  protected readonly pageCount = computed(() =>
    this.users.error()
      ? 1
      : Math.max(1, Math.ceil((this.users.value()?.totalCount ?? 0) / this.pageSize)),
  );
  protected readonly historyPageCount = computed(() =>
    this.transactions.error()
      ? 1
      : Math.max(1, Math.ceil((this.transactions.value()?.totalCount ?? 0) / this.pageSize)),
  );

  protected selectUser(user: ExplorerWalletDto): void {
    if (this.pending()) return;
    this.selectedUser.set(user);
    this.historyPage.set(1);
    this.error.set(null);
    this.success.set(null);
  }

  protected changePage(change: number): void {
    const page = this.page() + change;
    if (this.pending() || page < 1 || page > this.pageCount()) return;
    this.page.set(page);
    this.selectedUser.set(null);
    this.historyPage.set(1);
    this.error.set(null);
    this.success.set(null);
  }

  protected changeHistoryPage(change: number): void {
    const page = this.historyPage() + change;
    if (this.pending() || page < 1 || page > this.historyPageCount()) return;
    this.historyPage.set(page);
  }

  protected retry(): void {
    this.users.reload();
    this.transactions.reload();
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    const user = this.selectedUser();
    if (!user || this.pending() || !this.paymentForm().valid()) return;
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.commands.credit(user.userId, { amount: this.paymentForm.amount().value() });
      this.paymentForm().reset({ amount: 1 });
      this.historyPage.set(1);
      this.users.reload();
      this.transactions.reload();
      this.success.set('Uplata je evidentirana. EC su odmah raspolozivi.');
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Uplatu nije moguce evidentirati. Pokusaj ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}

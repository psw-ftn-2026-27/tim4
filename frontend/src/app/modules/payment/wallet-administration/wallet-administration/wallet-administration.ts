import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required } from '@angular/forms/signals';
import { PageResult } from '../../../../shared/api/page-result';
import { serverMessage } from '../../../../shared/util/server-message';
import { ExplorerWalletDto, WalletTransactionDto } from '../../api/payment-api-types';
import { WalletTransactions } from '../../personal-wallet/wallet-transactions/wallet-transactions';
import { WalletAdministrationService } from '../wallet-administration';

// The server's largest page; the search, the "Bez EC" filter and the credit for all of them
// work within it, because the server has no search yet. The page says so when there are more.
const PAGE_SIZE = 100;
const HISTORY_PAGE_SIZE = 20;

// The administrator's wallets: the one job before every demo is giving new tourists EC, so the
// tourists with none are counted on top with one button for all of them. The list filters as
// it is typed into; a click on a tourist opens their wallet beside the list.
@Component({
  imports: [FormField, WalletTransactions],
  selector: 'app-wallet-administration',
  styleUrl: './wallet-administration.scss',
  templateUrl: './wallet-administration.html',
})
export class WalletAdministration {
  private readonly commands = inject(WalletAdministrationService);

  protected readonly page = signal(1);
  protected readonly historyPage = signal(1);
  protected readonly selectedId = signal<string | null>(null);
  protected readonly filter = signal<'all' | 'empty'>('all');
  protected readonly quickAmounts = [50, 100, 200];
  protected readonly bulkAmount = signal(100);
  protected readonly confirmingBulk = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected readonly search = form(signal({ query: '' }));
  protected readonly paymentForm = form(signal({ amount: 100 as number | null }), (path) => {
    required(path.amount, { message: 'Unesite iznos.' });
    min(path.amount, 1, { message: 'Iznos mora biti pozitivan ceo broj EC.' });
    max(path.amount, 2147483647, { message: 'Iznos je prevelik.' });
  });

  protected readonly users = httpResource<PageResult<ExplorerWalletDto>>(
    () => `/api/payment/wallet/users?page=${this.page()}&pageSize=${PAGE_SIZE}`,
  );
  protected readonly transactions = httpResource<PageResult<WalletTransactionDto>>(() => {
    const id = this.selectedId();
    return id === null
      ? undefined
      : `/api/payment/wallet/users/${id}/transactions?page=${this.historyPage()}&pageSize=${HISTORY_PAGE_SIZE}`;
  });

  protected readonly people = computed(() => this.users.value()?.items ?? []);
  protected readonly empty = computed(() => this.people().filter((user) => user.balance === 0));
  protected readonly visible = computed(() => {
    const query = this.search.query().value().trim().toLowerCase();
    return (this.filter() === 'empty' ? this.empty() : this.people()).filter((user) =>
      user.email.toLowerCase().includes(query),
    );
  });
  protected readonly selected = computed(
    () => this.people().find((user) => user.userId === this.selectedId()) ?? null,
  );
  protected readonly amount = computed(() => this.paymentForm.amount().value() ?? 0);

  // More tourists than one page: the "Bez EC" count and the search cover only this page.
  protected readonly onePageOfMany = computed(() => this.pageCount() > 1);

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.users.value()?.totalCount ?? 0) / PAGE_SIZE)),
  );
  protected readonly historyPageCount = computed(() =>
    Math.max(1, Math.ceil((this.transactions.value()?.totalCount ?? 0) / HISTORY_PAGE_SIZE)),
  );

  protected select(user: ExplorerWalletDto): void {
    this.selectedId.set(user.userId);
    this.historyPage.set(1);
    this.error.set(null);
  }

  protected setAmount(value: number): void {
    this.paymentForm.amount().value.set(value);
  }

  protected changePage(change: number): void {
    const page = this.page() + change;
    if (page >= 1 && page <= this.pageCount()) {
      this.page.set(page);
      this.selectedId.set(null);
    }
  }

  protected changeHistoryPage(change: number): void {
    const page = this.historyPage() + change;
    if (page >= 1 && page <= this.historyPageCount()) {
      this.historyPage.set(page);
    }
  }

  protected async credit(event: Event): Promise<void> {
    event.preventDefault();
    const user = this.selected();
    if (user === null || this.pending()) {
      return;
    }
    if (!this.paymentForm().valid()) {
      this.paymentForm().markAsTouched();
      return;
    }
    const amount = this.amount();
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.commands.credit(user.userId, { amount });
      this.notice.set(`${user.email} je dobio ${amount} EC. EC su odmah raspoloživi.`);
      this.historyPage.set(1);
      this.users.reload();
      this.transactions.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Uplata nije evidentirana. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  // One credit per tourist without EC, one after another; the server has no command for many
  // yet. A second click while it runs does nothing. If some fail, the others keep their EC and
  // a new click credits only those still without EC, so nobody is credited twice.
  protected async creditAllEmpty(): Promise<void> {
    if (this.pending()) {
      return;
    }
    const people = this.empty();
    const amount = this.bulkAmount();
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    let done = 0;
    try {
      for (const user of people) {
        try {
          await this.commands.credit(user.userId, { amount });
          done++;
        } catch {
          // Counted below; the others still get their EC.
        }
      }
    } finally {
      this.pending.set(false);
      this.confirmingBulk.set(false);
      this.users.reload();
      this.transactions.reload();
    }
    if (done === people.length) {
      this.notice.set(`Svaki od ${done} turista je dobio ${amount} EC.`);
    } else {
      this.error.set(`Uplaćeno ${done} od ${people.length}. Pokušajte ponovo za ostale.`);
    }
  }
}

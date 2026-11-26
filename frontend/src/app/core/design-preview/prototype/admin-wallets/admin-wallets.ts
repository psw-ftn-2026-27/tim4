import { Component, computed, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { ProtoExplorer, explorers } from '../mock';

type Filter = 'all' | 'empty';

interface Credit {
  amount: number;
  date: string;
}

// PROTOTYPE of scenario S8. The administrator's real job before a demo is "give EC to the people
// who just signed up". Today that is: page through all tourists, press "Izaberi", type an amount,
// one by one. Here new tourists without EC are a filter and a banner with one bulk action, and a
// chosen tourist opens a panel with quick amounts and their history.
@Component({
  imports: [FormField],
  selector: 'app-admin-wallets',
  styleUrl: './admin-wallets.scss',
  templateUrl: './admin-wallets.html',
})
export class AdminWallets {
  protected readonly quickAmounts = [50, 100, 200, 500];

  protected readonly people = signal<ProtoExplorer[]>(explorers.map((e) => ({ ...e })));
  protected readonly credits = signal<Record<string, Credit[]>>({
    u1: [
      { amount: 100, date: '18.11.2026.' },
      { amount: 85, date: '14.11.2026.' },
    ],
    u2: [{ amount: 340, date: '15.11.2026.' }],
  });

  protected readonly search = form(signal({ query: '' }));
  protected readonly filter = signal<Filter>('all');
  protected readonly selectedId = signal<string | null>(null);
  protected readonly amount = form(signal({ value: 100 as number | null }));
  protected readonly bulkAmount = signal(100);
  protected readonly confirmingBulk = signal(false);
  protected readonly notice = signal<string | null>(null);

  protected readonly empty = computed(() => this.people().filter((p) => p.balance === 0));

  protected readonly visible = computed(() => {
    const query = this.search.query().value().trim().toLowerCase();
    return this.people().filter(
      (p) =>
        (this.filter() === 'all' || p.balance === 0) &&
        (query === '' || p.email.toLowerCase().includes(query)),
    );
  });

  protected readonly selected = computed(
    () => this.people().find((p) => p.id === this.selectedId()) ?? null,
  );
  protected readonly selectedCredits = computed(() => {
    const id = this.selectedId();
    return id === null ? [] : (this.credits()[id] ?? []);
  });

  protected readonly amountValue = computed(() => {
    const value = this.amount.value().value();
    return value === null || Number.isNaN(value) ? 0 : value;
  });

  protected select(id: string): void {
    this.selectedId.set(id);
    this.notice.set(null);
    this.amount().reset({ value: 100 });
  }

  protected setAmount(value: number): void {
    this.amount.value().value.set(value);
  }

  protected credit(event: Event): void {
    event.preventDefault();
    const person = this.selected();
    const amount = this.amountValue();
    if (person === null || amount <= 0) {
      return;
    }
    this.give([person.id], amount);
    this.notice.set(
      `Uplaćeno ${amount} EC na ${person.email}. Novo stanje: ${person.balance + amount} EC.`,
    );
  }

  protected creditAllEmpty(): void {
    const ids = this.empty().map((p) => p.id);
    this.give(ids, this.bulkAmount());
    this.confirmingBulk.set(false);
    this.notice.set(`Svaki od ${ids.length} turista je dobio ${this.bulkAmount()} EC.`);
  }

  private give(ids: string[], amount: number): void {
    this.people.update((people) =>
      people.map((p) =>
        ids.includes(p.id) ? { ...p, balance: p.balance + amount, lastCredit: '21.11.2026.' } : p,
      ),
    );
    this.credits.update((credits) => {
      const next = { ...credits };
      for (const id of ids) {
        next[id] = [{ amount, date: '21.11.2026.' }, ...(next[id] ?? [])];
      }
      return next;
    });
  }
}

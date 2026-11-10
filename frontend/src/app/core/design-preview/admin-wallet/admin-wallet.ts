import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface WalletUser {
  username: string;
  balance: number;
}

interface AdminTransaction {
  id: number;
  username: string;
  kind: 'Uplata' | 'Kupovina';
  amount: number;
  hoursAgo: number;
  reversed: boolean;
}

// Mock data chosen so that every storno rule shows up once.
const USERS: WalletUser[] = [
  { username: 'ana.petrovic', balance: 120 },
  { username: 'marko.jovic', balance: 30 },
  { username: 'jelena.ilic', balance: 260 },
];

const TRANSACTIONS: AdminTransaction[] = [
  { id: 1, username: 'ana.petrovic', kind: 'Uplata', amount: 100, hoursAgo: 2, reversed: false },
  { id: 2, username: 'marko.jovic', kind: 'Uplata', amount: 50, hoursAgo: 5, reversed: false },
  { id: 3, username: 'jelena.ilic', kind: 'Uplata', amount: 200, hoursAgo: 72, reversed: false },
  { id: 4, username: 'ana.petrovic', kind: 'Uplata', amount: 40, hoursAgo: 20, reversed: true },
  { id: 5, username: 'marko.jovic', kind: 'Kupovina', amount: -20, hoursAgo: 4, reversed: false },
];

@Component({
  imports: [RouterLink],
  selector: 'app-admin-wallet',
  styleUrl: './admin-wallet.scss',
  templateUrl: './admin-wallet.html',
})
export class AdminWallet {
  protected readonly users = USERS;
  protected readonly transactions = TRANSACTIONS;

  // Returns why storno is not offered, or null when it is.
  protected stornoBlockedReason(transaction: AdminTransaction): string | null {
    if (transaction.kind !== 'Uplata') {
      return 'Storno važi samo za uplate.';
    }
    if (transaction.hoursAgo >= 24) {
      return 'Uplata je starija od 24h.';
    }
    const balance = this.users.find((user) => user.username === transaction.username)?.balance ?? 0;
    if (balance - transaction.amount < 0) {
      return `Stanje bi postalo negativno (${balance} − ${transaction.amount} EC).`;
    }
    return null;
  }
}

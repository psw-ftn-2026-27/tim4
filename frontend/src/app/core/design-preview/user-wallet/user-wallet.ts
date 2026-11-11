import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface WalletTransaction {
  id: number;
  time: string;
  description: string;
  amount: number;
  reversed: boolean;
}

// Mock data: the balance is the sum of the transactions that are not reversed.
const TRANSACTIONS: WalletTransaction[] = [
  { id: 1, time: '10.11.2026. 12:05', description: 'Uplata administratora', amount: 100, reversed: false },
  { id: 2, time: '09.11.2026. 18:30', description: 'Kupovina ture „Fruškogorske staze“', amount: -30, reversed: false },
  { id: 3, time: '08.11.2026. 09:15', description: 'Uplata administratora', amount: 40, reversed: true },
  { id: 4, time: '01.11.2026. 16:40', description: 'Uplata administratora', amount: 50, reversed: false },
];

@Component({
  imports: [RouterLink],
  selector: 'app-user-wallet',
  styleUrl: './user-wallet.scss',
  templateUrl: './user-wallet.html',
})
export class UserWallet {
  protected readonly balance = 120;
  protected readonly transactions = TRANSACTIONS;
}

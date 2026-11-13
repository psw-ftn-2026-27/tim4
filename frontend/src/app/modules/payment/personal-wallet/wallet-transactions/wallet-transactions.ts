import { Component, input } from '@angular/core';
import { WalletTransactionDto } from '../../api/payment-api-types';

@Component({
  imports: [],
  selector: 'app-wallet-transactions',
  styleUrl: './wallet-transactions.scss',
  templateUrl: './wallet-transactions.html',
})
export class WalletTransactions {
  readonly transactions = input.required<WalletTransactionDto[]>();

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('sr-Latn-RS');
  }
}

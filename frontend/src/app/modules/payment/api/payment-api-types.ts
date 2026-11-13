export type TransactionStatus = 'Available' | 'Cancelled';

export interface WalletBalanceDto {
  availableBalance: number;
}

export interface WalletTransactionDto {
  id: string;
  amount: number;
  submittedAt: string;
  administratorId: string;
  status: TransactionStatus;
  cancelledByAdministratorId: string | null;
  cancelledAt: string | null;
}

export interface WalletTransactionSearchDto {
  status: TransactionStatus | null;
}

import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CreditWalletDto } from '../api/payment-api-types';

@Injectable({ providedIn: 'root' })
export class WalletAdministrationService {
  private readonly http = inject(HttpClient);

  async credit(userId: string, dto: CreditWalletDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`/api/payment/wallet/users/${userId}/payments`, dto));
  }
}

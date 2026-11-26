import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BarRefresh } from '../../../shared/bar/bar-refresh';
import { BuyGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class PersonalGiftCards {
  private readonly http = inject(HttpClient);
  private readonly barRefresh = inject(BarRefresh);

  async buyGiftCard(dto: BuyGiftCardDto): Promise<string> {
    const id = await firstValueFrom(this.http.post<string>(BASE_URL, dto));
    // The card's EC leave the wallet at once, so the balance in the bar changes.
    this.barRefresh.refresh();
    return id;
  }
}

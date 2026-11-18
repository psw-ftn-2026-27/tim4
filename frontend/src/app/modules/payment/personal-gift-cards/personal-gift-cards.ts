import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BuyGiftCardDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/gift-cards';

@Injectable({ providedIn: 'root' })
export class PersonalGiftCards {
  private readonly http = inject(HttpClient);

  buyGiftCard(dto: BuyGiftCardDto): Promise<string> {
    return firstValueFrom(this.http.post<string>(BASE_URL, dto));
  }
}

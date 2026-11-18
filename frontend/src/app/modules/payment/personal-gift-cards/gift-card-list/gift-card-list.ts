import { Component, input } from '@angular/core';
import { GiftCardDto, GiftCardStatus } from '../../api/payment-api-types';

@Component({
  selector: 'app-gift-card-list',
  templateUrl: './gift-card-list.html',
  styleUrl: './gift-card-list.scss',
})
export class GiftCardList {
  readonly cards = input.required<GiftCardDto[]>();
  // Bought cards show who received them, received cards show who gave them.
  readonly showGiver = input(false);

  // Gift card dates follow UTC on the server, so the date part of an ISO value is shown as it is.
  protected formatDate(value: string): string {
    const [year, month, day] = value.slice(0, 10).split('-');
    return `${day}.${month}.${year}.`;
  }

  protected statusLabel(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return '● Važeća';
      case 'Redeemed':
        return '✓ Iskorišćena';
      default:
        return '⊘ Istekla';
    }
  }

  protected statusClass(status: GiftCardStatus): string {
    switch (status) {
      case 'Valid':
        return 'badge badge--published';
      case 'Redeemed':
        return 'badge badge--muted';
      default:
        return 'badge badge--retired';
    }
  }
}

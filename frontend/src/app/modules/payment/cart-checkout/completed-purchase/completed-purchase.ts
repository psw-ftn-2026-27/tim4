import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatDateTime } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import { CompletedPurchaseDto, WalletBalanceDto } from '../../api/payment-api-types';
import { CheckoutSteps } from '../checkout-steps/checkout-steps';

// A confirmation is a moment, not a form: one centred column, a clear "done", and the tours the
// tourist just bought as the next step. The receipt is folded under them.
@Component({
  selector: 'app-completed-purchase',
  imports: [CheckoutSteps, RouterLink],
  templateUrl: './completed-purchase.html',
  styleUrl: './completed-purchase.scss',
})
export class CompletedPurchase {
  readonly purchaseId = input.required<string>();

  protected readonly purchase = httpResource<CompletedPurchaseDto>(
    () => `/api/payment/cart/purchases/${this.purchaseId()}`,
  );
  protected readonly wallet = httpResource<WalletBalanceDto>(() => '/api/payment/wallet/balance');

  protected readonly loadError = computed(() =>
    serverMessage(this.purchase.error(), 'Kupovina nije učitana.'),
  );

  // What the coupon and the gift card took off the price.
  protected readonly saved = computed(() => {
    const purchase = this.purchase.value();
    return (purchase?.appliedCoupon?.discount ?? 0) + (purchase?.appliedGiftCard?.discount ?? 0);
  });

  protected readonly time = computed(() => {
    const purchase = this.purchase.value();
    return purchase === undefined ? '' : formatDateTime(purchase.purchasedAt);
  });

  protected readonly toursText = computed(() => {
    const count = this.purchase.value()?.items.length ?? 0;
    return count === 1 ? 'turu' : `${count} ture`;
  });
}

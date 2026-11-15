import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CompletedPurchaseDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-completed-purchase',
  imports: [RouterLink],
  templateUrl: './completed-purchase.html',
  styleUrl: './completed-purchase.scss',
})
export class CompletedPurchase {
  readonly purchaseId = input.required<string>();
  protected readonly auth = inject(Auth);
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly purchase = httpResource<CompletedPurchaseDto>(() =>
    this.isExplorer() ? `/api/payment/cart/purchases/${this.purchaseId()}` : undefined,
  );
  protected readonly loadError = computed(() =>
    serverMessage(this.purchase.error(), 'Nije moguce ucitati detalje kupovine.'),
  );

  protected formatTime(value: string): string {
    return new Date(value).toLocaleString('sr-Latn-RS');
  }
}

import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, max, min, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { OwnerCouponDto, OwnerProductDto } from '../../api/payment-api-types';
import { OwnerCoupons } from '../owner-coupons';

interface CouponForm {
  discountPercent: number | null;
  validUntil: string;
  productId: string;
}

// An empty productId is the "Svi moji proizvodi" option; it is sent as null.
const EMPTY_COUPON: CouponForm = { discountPercent: null, validUntil: '', productId: '' };

// Today's date in the yyyy-mm-dd format of a date input, so the two compare as strings.
function today(): string {
  const date = new Date();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-my-coupons',
  styleUrl: './my-coupons.scss',
  templateUrl: './my-coupons.html',
})
export class MyCoupons {
  private readonly ownerCoupons = inject(OwnerCoupons);
  protected readonly auth = inject(Auth);

  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly coupons = httpResource<OwnerCouponDto[]>(() =>
    this.isExplorer() ? '/api/payment/coupons' : undefined,
  );
  protected readonly products = httpResource<OwnerProductDto[]>(() =>
    this.isExplorer() ? '/api/payment/coupons/products' : undefined,
  );

  protected readonly couponForm = form(signal<CouponForm>({ ...EMPTY_COUPON }), (path) => {
    required(path.discountPercent, { message: 'Unesite procenat popusta.' });
    min(path.discountPercent, 1, { message: 'Procenat mora biti najmanje 1.' });
    max(path.discountPercent, 100, { message: 'Procenat može biti najviše 100.' });
    validate(path.discountPercent, ({ value }) =>
      value() !== null && !Number.isInteger(value())
        ? { kind: 'integer', message: 'Procenat mora biti ceo broj.' }
        : undefined,
    );
    required(path.validUntil, { message: 'Unesite datum do kog kupon važi.' });
    validate(path.validUntil, ({ value }) =>
      value() !== '' && value() < today()
        ? { kind: 'past', message: 'Datum važenja ne može biti u prošlosti.' }
        : undefined,
    );
  });

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected formatDate(value: string): string {
    const [year, month, day] = value.split('-');
    return `${day}.${month}.${year}.`;
  }

  protected reload(): void {
    this.coupons.reload();
    this.products.reload();
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.pending()) return;
    if (!this.couponForm().valid()) {
      this.couponForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const productId = this.couponForm.productId().value();
      await this.ownerCoupons.createCoupon({
        discountPercent: this.couponForm.discountPercent().value() ?? 0,
        validUntil: this.couponForm.validUntil().value(),
        productId: productId === '' ? null : productId,
      });
      this.couponForm().reset({ ...EMPTY_COUPON });
      this.coupons.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kupon nije napravljen. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}

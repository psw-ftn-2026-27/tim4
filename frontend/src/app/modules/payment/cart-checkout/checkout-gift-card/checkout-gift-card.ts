import { Component, OnInit, computed, input, output, signal } from '@angular/core';
import { FormField, disabled, form } from '@angular/forms/signals';
import { AvailableGiftCardDto, CheckoutGiftCardDto } from '../../api/payment-api-types';

@Component({
  selector: 'app-checkout-gift-card',
  imports: [FormField],
  templateUrl: './checkout-gift-card.html',
  styleUrl: './checkout-gift-card.scss',
})
export class CheckoutGiftCard implements OnInit {
  readonly applied = input<CheckoutGiftCardDto | null>(null);
  readonly available = input<AvailableGiftCardDto | null>(null);
  readonly pending = input(false);
  readonly error = input<string | null>(null);
  readonly useChange = output<boolean>();

  private readonly model = signal({ use: false });
  protected readonly fields = form(this.model, (path) => {
    disabled(path.use, { when: () => this.pending() });
  });
  protected readonly lostRest = computed(() => {
    const giftCard = this.applied();
    return giftCard?.isValid ? giftCard.amount - giftCard.discount : 0;
  });

  // The page shows a fresh copy of this section after every load of the checkout, so the checkbox
  // starts from what the server saved.
  ngOnInit(): void {
    this.model.set({ use: this.applied() !== null });
  }

  protected changeUse(): void {
    this.useChange.emit(this.fields.use().value());
  }

  protected formatDate(value: string): string {
    const [year, month, day] = value.split('-');
    return `${day}.${month}.${year}.`;
  }
}

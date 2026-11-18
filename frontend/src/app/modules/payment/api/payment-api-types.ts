export type TransactionStatus = 'Available' | 'Cancelled';
export type TransactionKind = 'Credit' | 'Purchase' | 'GiftCard';

export interface WalletBalanceDto {
  availableBalance: number;
}

export interface WalletTransactionDto {
  id: string;
  amount: number;
  submittedAt: string;
  administratorId: string | null;
  status: TransactionStatus;
  cancelledByAdministratorId: string | null;
  cancelledAt: string | null;
  kind: TransactionKind;
  purchaseId: string | null;
  entityId: string | null;
  giftCardId: string | null;
}

export interface WalletTransactionSearchDto {
  status: TransactionStatus | null;
}

export interface ExplorerWalletDto {
  userId: string;
  email: string;
  balance: number;
}

export interface CreditWalletDto {
  amount: number;
}

export interface AddToCartDto {
  entityId: string;
}

export interface CartItemDto {
  id: string;
  productId: string;
  entityId: string;
  name: string;
  price: number;
}

export interface CartDto {
  id: string | null;
  items: CartItemDto[];
  total: number;
}

export interface CartCheckoutDto {
  id: string;
  items: CartItemDto[];
  total: number;
  availableBalance: number;
  appliedCoupon: CheckoutCouponDto | null;
  totalToPay: number;
  appliedGiftCard: CheckoutGiftCardDto | null;
  availableGiftCard: AvailableGiftCardDto | null;
}

export interface CheckoutGiftCardDto {
  giftCardId: string;
  amount: number;
  discount: number;
  isValid: boolean;
  invalidReason: string | null;
}

export interface AvailableGiftCardDto {
  giftCardId: string;
  amount: number;
  validUntil: string;
}

export interface CheckoutCouponDto {
  couponId: string;
  code: string;
  productId: string;
  discountPercent: number;
  discount: number;
  isValid: boolean;
  invalidReason: string | null;
}

export interface ApplyCouponDto {
  code: string;
}

export interface ConfirmCheckoutDto {
  totalToPay: number;
}

export interface PurchasedItemDto {
  id: string;
  productId: string;
  entityId: string;
  name: string;
  price: number;
}

export interface CompletedPurchaseDto {
  id: string;
  purchasedAt: string;
  items: PurchasedItemDto[];
  total: number;
  appliedCoupon: PurchaseCouponDto | null;
  appliedGiftCard: PurchaseGiftCardDto | null;
}

export interface PurchaseGiftCardDto {
  giftCardId: string;
  amount: number;
  discount: number;
}

export interface PurchaseCouponDto {
  code: string;
  productId: string;
  discountPercent: number;
  discount: number;
}

export interface OwnerCouponDto {
  code: string;
  discountPercent: number;
  validUntil: string;
  productName: string | null;
  isValid: boolean;
}

export interface OwnerProductDto {
  id: string;
  name: string;
}

export interface CreateCouponDto {
  discountPercent: number;
  validUntil: string;
  productId: string | null;
}

export type GiftCardStatus = 'Valid' | 'Redeemed' | 'Expired';

export interface GiftCardDto {
  id: string;
  giverId: string;
  giverEmail: string | null;
  recipientId: string;
  recipientEmail: string | null;
  amount: number;
  message: string;
  createdAt: string;
  validUntil: string;
  status: GiftCardStatus;
}

export interface MyGiftCardsDto {
  bought: GiftCardDto[];
  received: GiftCardDto[];
}

export interface BuyGiftCardDto {
  recipientEmail: string;
  amount: number;
  message: string;
}

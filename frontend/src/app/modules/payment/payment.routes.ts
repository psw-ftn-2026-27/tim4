import { Routes } from '@angular/router';
import { PersonalWallet } from './personal-wallet/personal-wallet/personal-wallet';
import { WalletAdministration } from './wallet-administration/wallet-administration/wallet-administration';
import { Cart } from './cart-checkout/cart/cart';
import { Checkout } from './cart-checkout/checkout/checkout';
import { CompletedPurchase } from './cart-checkout/completed-purchase/completed-purchase';
import { MyCoupons } from './owner-coupons/my-coupons/my-coupons';
import { MyGiftCards } from './personal-gift-cards/my-gift-cards/my-gift-cards';

export const paymentRoutes: Routes = [
  { path: 'cart', component: Cart },
  { path: 'checkout/:checkoutId', component: Checkout },
  { path: 'purchases/:purchaseId', component: CompletedPurchase },
  { path: 'administration', component: WalletAdministration },
  { path: 'coupons', component: MyCoupons },
  { path: 'gift-cards', component: MyGiftCards },
  { path: '', component: PersonalWallet },
];

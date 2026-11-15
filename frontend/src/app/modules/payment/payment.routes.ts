import { Routes } from '@angular/router';
import { PersonalWallet } from './personal-wallet/personal-wallet/personal-wallet';
import { WalletAdministration } from './wallet-administration/wallet-administration/wallet-administration';
import { Cart } from './shopping-cart/cart/cart';
import { Checkout } from './shopping-cart/checkout/checkout';
import { CompletedPurchase } from './shopping-cart/completed-purchase/completed-purchase';

export const paymentRoutes: Routes = [
  { path: 'cart', component: Cart },
  { path: 'checkout/:checkoutId', component: Checkout },
  { path: 'purchases/:purchaseId', component: CompletedPurchase },
  { path: 'administration', component: WalletAdministration },
  { path: '', component: PersonalWallet },
];

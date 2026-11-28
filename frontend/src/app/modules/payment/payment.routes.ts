import { Routes } from '@angular/router';
import { PersonalWallet } from './personal-wallet/personal-wallet/personal-wallet';
import { WalletAdministration } from './wallet-administration/wallet-administration/wallet-administration';
import { Cart } from './cart-checkout/cart/cart';
import { Checkout } from './cart-checkout/checkout/checkout';
import { CompletedPurchase } from './cart-checkout/completed-purchase/completed-purchase';
import { MyCoupons } from './owner-coupons/my-coupons/my-coupons';
import { MyGiftCards } from './personal-gift-cards/my-gift-cards/my-gift-cards';
import { requireRole } from '../../core/auth/require-role';

export const paymentRoutes: Routes = [
  { path: 'cart', component: Cart, canActivate: [requireRole('explorer')] },
  { path: 'checkout/:checkoutId', component: Checkout, canActivate: [requireRole('explorer')] },
  {
    path: 'purchases/:purchaseId',
    component: CompletedPurchase,
    canActivate: [requireRole('explorer')],
  },
  {
    path: 'administration',
    component: WalletAdministration,
    canActivate: [requireRole('administrator')],
  },
  { path: 'coupons', component: MyCoupons, canActivate: [requireRole('explorer')] },
  { path: 'gift-cards', component: MyGiftCards, canActivate: [requireRole('explorer')] },
  { path: '', component: PersonalWallet, canActivate: [requireRole()] },
];

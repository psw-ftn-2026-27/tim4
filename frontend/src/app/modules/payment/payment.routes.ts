import { Routes } from '@angular/router';
import { PersonalWallet } from './personal-wallet/personal-wallet/personal-wallet';
import { WalletAdministration } from './wallet-administration/wallet-administration/wallet-administration';

export const paymentRoutes: Routes = [
  { path: 'administration', component: WalletAdministration },
  { path: '', component: PersonalWallet },
];

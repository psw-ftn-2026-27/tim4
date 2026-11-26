import { Routes } from '@angular/router';
import { AdminChallengesProto } from './admin-challenges-proto/admin-challenges-proto';
import { AdminStart } from './admin-start/admin-start';
import { GuestHome } from './guest-home/guest-home';
import { GuestLogin } from './guest-login/guest-login';
import { GuestRegister } from './guest-register/guest-register';
import { GuestShell } from './guest-shell/guest-shell';
import { TouristStart } from './tourist-start/tourist-start';
import { AdminWallets } from './admin-wallets/admin-wallets';
import { AdminShell } from './admin-shell/admin-shell';
import { ProtoIndex } from './proto-index/proto-index';
import { ProtoTodo } from './proto-todo/proto-todo';
import { TouristChallenges } from './tourist-challenges/tourist-challenges';
import { TouristCart } from './tourist-cart/tourist-cart';
import { TouristCheckout } from './tourist-checkout/tourist-checkout';
import { TouristPurchase } from './tourist-purchase/tourist-purchase';
import { TouristActiveTour } from './tourist-active-tour/tourist-active-tour';
import { TouristJourneys } from './tourist-journeys/tourist-journeys';
import { TouristStudio } from './tourist-studio/tourist-studio';
import { TouristTourEditor } from './tourist-tour-editor/tourist-tour-editor';
import { TouristGiftCards } from './tourist-gift-cards/tourist-gift-cards';
import { TouristWallet } from './tourist-wallet/tourist-wallet';
import { TouristShell } from './tourist-shell/tourist-shell';
import { TouristTourDetails } from './tourist-tour-details/tourist-tour-details';
import { TouristTours } from './tourist-tours/tourist-tours';
import { TouristQuiz } from './tourist-quiz/tourist-quiz';

// A screen of the new navigation that is not drawn yet.
const todo = (path: string, title: string, plan: string) => ({
  path,
  component: ProtoTodo,
  data: { title, plan },
});

// The redesign prototypes, under /design-preview/prototype. Mock data only.
export const prototypeRoutes: Routes = [
  { path: '', component: ProtoIndex },
  {
    path: 'guest',
    component: GuestShell,
    children: [
      { path: '', component: GuestHome },
      { path: 'login', component: GuestLogin },
      { path: 'register', component: GuestRegister },
    ],
  },
  {
    path: 'tourist',
    component: TouristShell,
    children: [
      { path: '', component: TouristStart },
      { path: 'tours', component: TouristTours },
      { path: 'tours/:id', component: TouristTourDetails },
      { path: 'challenges', component: TouristChallenges },
      { path: 'quiz', component: TouristQuiz },
      { path: 'journeys', component: TouristJourneys },
      { path: 'active-tour', component: TouristActiveTour },
      { path: 'studio', component: TouristStudio },
      { path: 'studio/:id', component: TouristTourEditor },
      { path: 'cart', component: TouristCart },
      { path: 'checkout', component: TouristCheckout },
      { path: 'purchase', component: TouristPurchase },
      { path: 'wallet', component: TouristWallet },
      todo('preferences', 'Preference tura', 'Jedna forma, iz menija korisnika.'),
      { path: 'gift-cards', component: TouristGiftCards },
    ],
  },
  {
    path: 'admin',
    component: AdminShell,
    children: [
      { path: '', component: AdminStart },
      { path: 'challenges', component: AdminChallengesProto },
      { path: 'wallets', component: AdminWallets },
      todo('tours', 'Ture', 'Pregled objavljenih tura, samo za čitanje.'),
    ],
  },
];

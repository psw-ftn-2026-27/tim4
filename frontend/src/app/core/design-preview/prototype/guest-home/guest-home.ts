import { Component } from '@angular/core';
import { Closing } from '../../../home/closing/closing';
import { ExampleChallenges } from '../../../home/example-challenges/example-challenges';
import { ExampleTours } from '../../../home/example-tours/example-tours';
import { Hero } from '../../../home/hero/hero';
import { Ribbon } from '../../../home/ribbon/ribbon';
import { Steps } from '../../../home/steps/steps';

// PROTOTYPE. The real landing sections, unchanged, inside the guest shell. Their buttons lead to
// the real /register and /login; in the redesign only the bar around them changes.
@Component({
  imports: [Hero, Ribbon, Steps, ExampleTours, ExampleChallenges, Closing],
  selector: 'app-guest-home',
  template: `
    <app-hero />
    <app-ribbon />
    <app-steps />
    <app-example-tours />
    <app-example-challenges />
    <app-closing />
  `,
  styles: `
    app-hero,
    app-steps,
    app-example-tours,
    app-example-challenges,
    app-closing {
      display: block;
      margin-inline: auto;
      max-width: 66rem;
    }
  `,
})
export class GuestHome {}

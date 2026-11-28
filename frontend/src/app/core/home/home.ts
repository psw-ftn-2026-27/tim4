import { Component, computed, inject, input } from '@angular/core';
import { Auth } from '../auth/auth';
import { Closing } from './closing/closing';
import { ExampleChallenges } from './example-challenges/example-challenges';
import { ExampleTours } from './example-tours/example-tours';
import { Hero } from './hero/hero';
import { Ribbon } from './ribbon/ribbon';
import { Start } from './start/start';
import { Steps } from './steps/steps';
import { TouristStart } from './tourist-start/tourist-start';

// The page at `/`. A visitor gets the sections that invite them to register; a tourist gets the
// start page that answers "what now?"; an administrator gets their two sections.
@Component({
  imports: [Hero, Ribbon, Steps, ExampleTours, ExampleChallenges, Closing, Start, TouristStart],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(Auth);

  // ?novi=1, set by the registration page, opens the tourist's first steps.
  readonly novi = input<string>();

  protected readonly name = computed(() => this.auth.user()?.email.split('@')[0] ?? '');
  protected readonly isAdmin = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}

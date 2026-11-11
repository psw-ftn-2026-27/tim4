import { Component, computed, inject } from '@angular/core';
import { Auth } from '../auth/auth';
import { Closing } from './closing/closing';
import { ExampleChallenges } from './example-challenges/example-challenges';
import { ExampleTours } from './example-tours/example-tours';
import { Hero } from './hero/hero';
import { Ribbon } from './ribbon/ribbon';
import { Start } from './start/start';
import { Steps } from './steps/steps';

// The page at `/`. A visitor gets the sections that invite them to register;
// a signed-in user gets a start page with links grouped by goal, matching their role.
@Component({
  imports: [Hero, Ribbon, Steps, ExampleTours, ExampleChallenges, Closing, Start],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(Auth);

  protected readonly name = computed(() => this.auth.user()?.email.split('@')[0] ?? '');
  protected readonly isAdmin = computed(
    () => this.auth.user()?.roles.includes('administrator') ?? false,
  );
}

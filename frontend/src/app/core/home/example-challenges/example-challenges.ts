import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Three illustrative challenges on a drawn map (P6): every one leads to /register.

interface ExampleChallenge {
  kind: string;
  title: string;
  description: string;
  xp: number;
}

@Component({
  imports: [RouterLink],
  selector: 'app-example-challenges',
  styleUrl: './example-challenges.scss',
  templateUrl: './example-challenges.html',
})
export class ExampleChallenges {
  // Order matters: challenge 1, 2, 3 match markers 1, 2, 3 on the map.
  protected readonly challenges: ExampleChallenge[] = [
    {
      kind: 'Kviz',
      title: 'Ko je sagradio tvrđavu?',
      description: 'Odgovori na pitanja o mestu na kom stojiš.',
      xp: 50,
    },
    {
      kind: 'Skrivena lokacija',
      title: 'Pronađi skrivenu česmu',
      description: 'Po tragovima na mapi otkrij mesto koje turisti promaše.',
      xp: 120,
    },
    {
      kind: 'Društveni izazov',
      title: 'Obiđite trg sa klubom',
      description: 'Zajedno sa članovima kluba stignite do cilja.',
      xp: 200,
    },
  ];
}

import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Three illustrative challenges on a drawn map (P6), one of each kind the Games module
// has: a quiz, a location challenge and a social challenge. Every one leads to /register.

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
      description:
        'Obiđi ulicu, pročitaj natpise i odgovori na pitanja. Što tačnije, to više XP-a.',
      xp: 50,
    },
    {
      kind: 'Lokacijski izazov',
      title: 'Gde je nastala ova fotografija?',
      description: 'Dobiješ fotografiju i krug na mapi. Pronađi mesto sa kog je snimljena.',
      xp: 120,
    },
    {
      kind: 'Društveni izazov',
      title: 'Okupite se na trgu',
      description:
        'Kad se između 18 i 20h okupi dovoljno ljudi na istom mestu, izazov je rešen za sve.',
      xp: 200,
    },
  ];
}

import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

// Three illustrative tours (P6): static examples, and every card leads to /register.

type Difficulty = 'easy' | 'moderate' | 'hard';

interface ExampleTour {
  name: string;
  excerpt: string;
  length: string;
  difficulty: Difficulty;
  illustration: string;
}

const DIFFICULTY_LABELS: Record<Difficulty, string> = {
  easy: 'Lako',
  moderate: 'Srednje',
  hard: 'Teško',
};

const DIFFICULTY_ICONS: Record<Difficulty, string> = {
  easy: '●○○',
  moderate: '●●○',
  hard: '●●●',
};

@Component({
  imports: [RouterLink],
  selector: 'app-example-tours',
  styleUrl: './example-tours.scss',
  templateUrl: './example-tours.html',
})
export class ExampleTours {
  protected readonly difficultyLabels = DIFFICULTY_LABELS;
  protected readonly difficultyIcons = DIFFICULTY_ICONS;

  protected readonly tours: ExampleTour[] = [
    {
      name: 'Tajne Petrovaradinske tvrđave',
      excerpt: 'Spusti se u podzemne galerije i saznaj zašto sat na tornju pokazuje obrnuto vreme.',
      length: '3,5 km',
      difficulty: 'easy',
      illustration: 'destination',
    },
    {
      name: 'Kalemegdan kroz vekove',
      excerpt: 'Od rimskog utvrđenja do parka: svaka kapija čuva deo priče o gradu na ušću.',
      length: '5 km',
      difficulty: 'moderate',
      illustration: 'travel-destination',
    },
    {
      name: 'Stazom starih vodenica',
      excerpt: 'Planinska staza uz potok, gde svaka vodenica otkriva kako se nekada živelo.',
      length: '11 km',
      difficulty: 'hard',
      illustration: 'hiking',
    },
  ];
}

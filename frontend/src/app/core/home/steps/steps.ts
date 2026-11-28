import { Component } from '@angular/core';

// "Kako funkcioniše": three static steps, nothing to click.
@Component({
  selector: 'app-steps',
  styleUrl: './steps.scss',
  templateUrl: './steps.html',
})
export class Steps {
  protected readonly steps = [
    {
      title: 'Izaberi turu',
      text: 'Pronađi turu po mestu, težini ili načinu prevoza.',
      illustration: 'finding-the-way',
    },
    {
      title: 'Kreni',
      text: 'Prati mapu od jedne ključne tačke do sledeće.',
      illustration: 'hiking',
    },
    {
      title: 'Otključaj priču na mestu',
      text: 'Kad stigneš do tačke, otvara se novi deo priče.',
      illustration: 'adventure-map',
    },
  ];
}

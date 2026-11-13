import { Component } from '@angular/core';

// "Kako funkcioniše": four static steps, nothing to click. A tour can be started only
// once it is bought, so buying is a step of its own.
@Component({
  selector: 'app-steps',
  styleUrl: './steps.scss',
  templateUrl: './steps.html',
})
export class Steps {
  protected readonly steps = [
    {
      title: 'Izaberi turu',
      text: 'Pretraži ture po mestu, težini, tagovima i ceni.',
      illustration: 'finding-the-way',
    },
    {
      title: 'Kupi je za EC',
      text: 'Plati Explorer Coin-ima iz novčanika; besplatne ture su odmah tvoje.',
      illustration: 'travel-destination',
    },
    {
      title: 'Kreni',
      text: 'Prati mapu od jedne ključne tačke do sledeće.',
      illustration: 'hiking',
    },
    {
      title: 'Otključaj tajnu na tački',
      text: 'Kad stigneš do tačke, otvara se deo priče koji je autor sakrio.',
      illustration: 'adventure-map',
    },
  ];
}

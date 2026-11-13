import { Component } from '@angular/core';

// A decorative band under the hero; screen readers skip it.
@Component({
  selector: 'app-ribbon',
  styleUrl: './ribbon.scss',
  templateUrl: './ribbon.html',
})
export class Ribbon {
  // The list is shown twice so the scrolling animation can loop without a gap.
  protected readonly items = [
    'Ture sa pričom',
    'Besplatne i plaćene ture',
    'Kviz',
    'Lokacijski izazov',
    'Društveni izazov',
    'XP za rešene izazove',
    'Ključne tačke',
  ];
}

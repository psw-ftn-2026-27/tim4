import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface SearchTour {
  id: number;
  name: string;
  excerpt: string;
  lengthKm: number;
  tags: string[];
  difficulty: 'Easy' | 'Moderate' | 'Hard';
  price: number;
  status: 'Draft' | 'Published' | 'Archived';
}

// Mock data includes tours that are not published; the search must not show them.
const ALL_TOURS: SearchTour[] = [
  {
    id: 1,
    name: 'Fruškogorske staze',
    excerpt: 'Šumska tura kroz Frušku goru sa pogledom na Dunav i posetom manastiru...',
    lengthKm: 12.4,
    tags: ['priroda', 'planinarenje'],
    difficulty: 'Moderate',
    price: 30,
    status: 'Published',
  },
  {
    id: 2,
    name: 'Stari grad Novog Sada',
    excerpt: 'Šetnja od Trga slobode do Dunavskog parka uz priče o istoriji grada...',
    lengthKm: 3.2,
    tags: ['istorija', 'grad'],
    difficulty: 'Easy',
    price: 0,
    status: 'Published',
  },
  {
    id: 3,
    name: 'Uspon na Rtanj',
    excerpt: 'Zahtevan uspon na vrh Šiljak sa noćenjem u planinarskom domu...',
    lengthKm: 18.9,
    tags: ['planinarenje', 'avantura'],
    difficulty: 'Hard',
    price: 60,
    status: 'Published',
  },
  {
    id: 4,
    name: 'Biciklom kroz Bačku',
    excerpt: 'Ruta u pripremi.',
    lengthKm: 40,
    tags: ['bicikl'],
    difficulty: 'Moderate',
    price: 20,
    status: 'Draft',
  },
  {
    id: 5,
    name: 'Zimska Tara',
    excerpt: 'Arhivirana sezonska tura.',
    lengthKm: 9.5,
    tags: ['priroda'],
    difficulty: 'Moderate',
    price: 25,
    status: 'Archived',
  },
];

@Component({
  imports: [RouterLink],
  selector: 'app-tour-search',
  styleUrl: './tour-search.scss',
  templateUrl: './tour-search.html',
})
export class TourSearch {
  protected readonly allCount = ALL_TOURS.length;
  protected readonly tours = ALL_TOURS.filter((tour) => tour.status === 'Published');
}

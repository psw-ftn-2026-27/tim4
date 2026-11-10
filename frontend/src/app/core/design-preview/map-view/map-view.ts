import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface MapPin {
  id: number;
  name: string;
  x: number;
  y: number;
}

// Mock data: x and y are positions on the static 400 x 240 drawing, not real coordinates.
const CHALLENGE_PINS: MapPin[] = [
  { id: 1, name: 'Zalazak na Petrovaradinu', x: 300, y: 150 },
  { id: 2, name: 'Istorija Novog Sada', x: 150, y: 80 },
  { id: 3, name: 'Mostovi na Dunavu', x: 230, y: 190 },
];

const TOUR_POINTS: MapPin[] = [
  { id: 1, name: 'Trg slobode', x: 70, y: 60 },
  { id: 2, name: 'Dunavski park', x: 170, y: 110 },
  { id: 3, name: 'Most slobode', x: 240, y: 180 },
  { id: 4, name: 'Petrovaradinska tvrđava', x: 330, y: 120 },
];

@Component({
  imports: [RouterLink],
  selector: 'app-map-view',
  styleUrl: './map-view.scss',
  templateUrl: './map-view.html',
})
export class MapView {
  protected readonly challengePins = CHALLENGE_PINS;
  protected readonly tourPoints = TOUR_POINTS;
  protected readonly tourPath = TOUR_POINTS.map((point) => `${point.x},${point.y}`).join(' ');
  protected readonly tourLengthKm = '4,8';
  protected readonly picked = { latitude: '45.25170', longitude: '19.86290', x: 210, y: 130 };
}

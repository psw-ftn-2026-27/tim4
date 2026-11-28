import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface AuthorTour {
  id: number;
  name: string;
  status: 'Draft' | 'Published' | 'Archived';
  archivedAt: string | null;
}

// Mock data: one tour in each state, so each state shows its own action.
const TOURS: AuthorTour[] = [
  { id: 1, name: 'Biciklom kroz Bačku', status: 'Draft', archivedAt: null },
  { id: 2, name: 'Fruškogorske staze', status: 'Published', archivedAt: null },
  { id: 3, name: 'Zimska Tara', status: 'Archived', archivedAt: '03.11.2026. u 18:42' },
];

@Component({
  imports: [RouterLink],
  selector: 'app-tour-editing',
  styleUrl: './tour-editing.scss',
  templateUrl: './tour-editing.html',
})
export class TourEditing {
  protected readonly tours = TOURS;
}
